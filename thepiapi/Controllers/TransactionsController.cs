using thepiapi.Data;
using thepiapi.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using thepiapi.Models;

namespace thepiapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ApplicationDbContext context, ILogger<TransactionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/transactions
        [HttpGet("list")]
        public async Task<IActionResult> GetTransactions([FromQuery] int? accountId, [FromQuery] int? month, [FromQuery] int? year)
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == UserId);

            if (accountId.HasValue)
                query = query.Where(t => t.AccountId == accountId.Value);

            if (month.HasValue && year.HasValue)
                query = query.Where(t => t.TransactionDate.Month == month && t.TransactionDate.Year == year);

            var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
            return Ok(transactions);
        } // GET: api/transactions/{id}
        [HttpGet("list/{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

                var transaction = await _context.Transactions
                    .Include(t => t.Account)
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId && t.Id == id)
                    .Select(t => new TransactionResponse
                    {
                        Id = t.Id,
                        AccountId = t.AccountId,
                        AccountName = t.Account.Name,
                        Amount = t.Amount,
                        Description = t.Description,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category.Name,
                        CategoryColor = t.Category.Color ?? "#6B7280",
                        CategoryIcon = t.Category.Icon ?? "tag",
                        TransactionDate = t.TransactionDate,
                        CreatedDate = t.CreatedDate,
                        Merchant = t.Merchant,
                        PaymentMethod = t.PaymentMethod,
                    })
                    .FirstOrDefaultAsync();

                if (transaction == null)
                    return NotFound(new { message = "Transaction not found" });

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transaction {id}");
                return StatusCode(500, new { message = "An error occurred while fetching the transaction" });
            }
        }

        // POST: api/transactions
        [HttpPost]
        public async Task<IActionResult> CreateTransaction(CreateTransactionRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

                // Verify account belongs to user
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId);

                if (account == null)
                    return BadRequest(new { message = "Account not found or access denied" });

                // Verify category exists
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                    return BadRequest(new { message = "Category not found" });

                var transaction = new thepiapi.Models.Transaction
                {
                    UserId = userId,
                    AccountId = request.AccountId,
                    Amount = request.Amount,
                    Description = request.Description,
                    CategoryId = request.CategoryId,
                    TransactionDate = request.TransactionDate,
                    Merchant = request.Merchant,
                    PaymentMethod = request.PaymentMethod,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);

                // Update account balance (if Account.Balance is double? or double)
                if (account.Balance == null)
                    account.Balance = 0;
                account.Balance += request.Amount;

                await _context.SaveChangesAsync();

                // Return the created transaction
                var createdTransaction = new TransactionResponse
                {
                    Id = transaction.Id,
                    AccountId = transaction.AccountId,
                    AccountName = account.Name,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    CategoryId = transaction.CategoryId,
                    CategoryName = category.Name,
                    CategoryColor = category.Color ?? "#6B7280",
                    CategoryIcon = category.Icon ?? "tag",
                    TransactionDate = transaction.TransactionDate,
                    CreatedDate = transaction.CreatedDate,
                    Merchant = transaction.Merchant,
                    PaymentMethod = transaction.PaymentMethod,
                };

                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, createdTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                return StatusCode(500, new { message = "An error occurred while creating the transaction" });
            }
        }

        // PUT: api/transactions/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionRequest request)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);

                if (transaction == null) return NotFound();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == transaction.AccountId && a.UserId == UserId);

                if (account != null)
                {
                    // ADJUSTMENT: Remove old amount, add new amount
                    // Example: Old -10, New -15. Balance 100 -> 100 - (-10) + (-15) = 95.
                    account.Balance = (account.Balance ?? 0) - transaction.Amount + request.Amount;
                }

                // Update transaction fields
                transaction.Amount = request.Amount;
                transaction.Description = request.Description;
                transaction.CategoryId = request.CategoryId;
                transaction.TransactionDate = request.TransactionDate;

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return Ok(new { message = "Transaction updated and balance adjusted", newBalance = account?.Balance });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
        // DELETE: api/transactions/{id}
        [HttpDelete("deletet/{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);

                if (transaction == null) return NotFound();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == transaction.AccountId && a.UserId == UserId);

                if (account != null)
                {
                    // ROLLBACK: Reverse the amount. 
                    // If it was -10 (expense), this ADDS 10. If +100 (income), it SUBTRACTS 100.
                    account.Balance = (account.Balance ?? 0) - transaction.Amount;
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return Ok(new { message = "Transaction deleted and balance restored", newBalance = account?.Balance });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
        // GET: api/transactions/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

                var query = _context.Transactions
                    .Where(t => t.UserId == userId);

                if (startDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= endDate.Value);

                var summary = await query
                    .GroupBy(t => 1)
                    .Select(g => new TransactionSummaryResponse
                    {
                        TotalIncome = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                        TotalExpenses = Math.Abs(g.Where(t => t.Amount < 0).Sum(t => t.Amount)),
                        NetFlow = g.Sum(t => t.Amount),
                        TransactionCount = g.Count()
                    })
                    .FirstOrDefaultAsync();

                return Ok(summary ?? new TransactionSummaryResponse());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction summary");
                return StatusCode(500, new { message = "An error occurred while fetching summary" });
            }
        }

        // GET: api/transactions/recent
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int count = 10)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

                var transactions = await _context.Transactions
                    .Include(t => t.Account)
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(count)
                    .Select(t => new TransactionResponse
                    {
                        Id = t.Id,
                        AccountId = t.AccountId,
                        AccountName = t.Account.Name,
                        Amount = t.Amount,
                        Description = t.Description,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category.Name,
                        CategoryColor = t.Category.Color ?? "#6B7280",
                        CategoryIcon = t.Category.Icon ?? "tag",
                        TransactionDate = t.TransactionDate,
                        CreatedDate = t.CreatedDate,
                        Merchant = t.Merchant,
                        PaymentMethod = t.PaymentMethod,
                    })
                    .ToListAsync();

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent transactions");
                return StatusCode(500, new { message = "An error occurred while fetching recent transactions" });
            }
        }

        // POST: api/transactions/quick-add
        [HttpPost("quick-add")]
        public async Task<IActionResult> QuickAddTransaction([FromBody] QuickAddTransactionRequest request)
        {
            if (UserId == 0) return Unauthorized();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == UserId && (a.IsActive == null || a.IsActive == true));

            if (account == null) return BadRequest(new { message = "No active account found." });

            // NEW: Look up the category ID by name
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == request.CategoryName && (c.UserId == UserId || c.UserId == null));

            var transaction = new Transaction
            {
                UserId = UserId,
                AccountId = account.Id,
                Amount = request.Amount,
                Description = request.Description,
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                // Use the ID we found, or fallback to 1 if not found
                CategoryId = category?.Id ?? 1
            };

            account.Balance = (account.Balance ?? 0.0) + request.Amount;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new TransactionResponse
            {
                Id = transaction.Id,
                AccountName = account.Name,
                Amount = transaction.Amount,
                Description = transaction.Description,
                CategoryName = category?.Name ?? "General",
                CategoryColor = category?.Color ?? "#6B7280",
                TransactionDate = transaction.TransactionDate
            });
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchTransactions([FromQuery] string? query, [FromQuery] DateOnly? start, [FromQuery] DateOnly? end)
        {
            var search = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.UserId == UserId);

            if (!string.IsNullOrEmpty(query))
            {
                search = search.Where(t => t.Description.Contains(query) || (t.Notes != null && t.Notes.Contains(query)));
            }

            if (start.HasValue) search = search.Where(t => t.TransactionDate >= start);
            if (end.HasValue) search = search.Where(t => t.TransactionDate <= end);

            var results = await search
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionResponse // MAP TO DTO HERE
                {
                    Id = t.Id,
                    AccountId = t.AccountId,
                    AccountName = t.Account.Name,
                    Amount = t.Amount,
                    Description = t.Description,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    CategoryColor = t.Category.Color,
                    CategoryIcon = t.Category.Icon,
                    TransactionDate = t.TransactionDate,
                    Notes = t.Notes
                })
                .ToListAsync();

            return Ok(results);
        }
        [HttpGet("export")]
        public async Task<IActionResult> ExportToCsv()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.UserId == UserId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Date,Description,Amount,Category,Account,Notes");

            foreach (var t in transactions)
            {
                csv.AppendLine($"{t.TransactionDate},{t.Description},{t.Amount},{t.Category?.Name},{t.Account?.Name},{t.Notes}");
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(buffer, "text/csv", $"Ledger_Backup_{DateTime.Now:yyyyMMdd}.csv");
        }

        private async Task<int> AutoDetectCategory(string description, int userId)
        {
            // Simple auto-detection logic
            var lowerDesc = description.ToLower();

            if (lowerDesc.Contains("salary") || lowerDesc.Contains("paycheck"))
                return await GetCategoryId("Salary", userId);

            if (lowerDesc.Contains("starbucks") || lowerDesc.Contains("coffee"))
                return await GetCategoryId("Coffee", userId);

            if (lowerDesc.Contains("walmart") || lowerDesc.Contains("grocer") || lowerDesc.Contains("food"))
                return await GetCategoryId("Groceries", userId);

            if (lowerDesc.Contains("netflix") || lowerDesc.Contains("spotify") || lowerDesc.Contains("subscription"))
                return await GetCategoryId("Subscriptions", userId);

            if (lowerDesc.Contains("rent") || lowerDesc.Contains("mortgage"))
                return await GetCategoryId("Rent/Mortgage", userId);

            if (lowerDesc.Contains("uber") || lowerDesc.Contains("lyft") || lowerDesc.Contains("taxi"))
                return await GetCategoryId("Transportation", userId);

            // Default to "Shopping" for negative amounts, "Other Income" for positive
            return await GetCategoryId("Shopping", userId);
        }

        private async Task<int> GetCategoryId(string categoryName, int userId)
        {
            // First try user's categories
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == categoryName && (c.UserId == userId || c.UserId == null));

            if (category != null) return category.Id;

            // If not found, get default "Shopping" category
            var defaultCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Shopping" && c.UserId == null);

            return defaultCategory?.Id ?? 1; // Fallback to first category
        }
    }
}
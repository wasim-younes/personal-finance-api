using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using thepiapi.Data;
using thepiapi.Models;
using thepiapi.Models.DTOs;

namespace thepiapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillsController> _logger;

        public BillsController(ApplicationDbContext context, ILogger<BillsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingBills()
        {
            try
            {
                var now = DateTime.Today;

                var bills = await _context.Bills
                    .Include(b => b.Category)
                    .Where(b => b.UserId == UserId && (b.IsActive == null || b.IsActive == true))
                    .OrderBy(b => b.NextDueDate) // Matches your model: NextDueDate
                    .ToListAsync();

                var response = bills.Select(b => new UpcomingBill
                {
                    Id = b.Id,
                    Name = b.Name, // Matches your model: Name
                    Amount = b.Amount,
                    DueDate = b.NextDueDate, // Maps your NextDueDate to the DTO
                    DaysUntilDue = (b.NextDueDate - now).Days,
                    CategoryName = b.Category?.Name ?? "General"
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upcoming bills");
                return StatusCode(500, new { message = "Error fetching bills" });
            }
        }

        [HttpPost("pay/{id}")]
        public async Task<IActionResult> PayBill(int id, [FromQuery] int accountId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == UserId);

                if (bill == null) return NotFound(new { message = "Bill not found" });
                if (account == null) return BadRequest(new { message = "Select a valid account" });

                // 1. Create a Manual Transaction Entry
                var newTransaction = new thepiapi.Models.Transaction
                {
                    UserId = UserId,
                    AccountId = account.Id,
                    Amount = -bill.Amount,
                    Description = $"Bill Paid: {bill.Name}", // Matches your model: Name
                    CategoryId = bill.CategoryId,
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    CreatedDate = DateTime.UtcNow
                };

                // 2. Subtract from your manual account balance
                account.Balance = (account.Balance ?? 0) - bill.Amount;

                // 3. Move the Bill for the next cycle
                // Since you have Frequency, we could make this smart, but for now we stick to monthly
                bill.NextDueDate = bill.NextDueDate.AddMonths(1);

                _context.Transactions.Add(newTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Payment recorded successfully",
                    newBalance = account.Balance,
                    nextDueDate = bill.NextDueDate
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error processing payment for bill {id}");
                return StatusCode(500, new { message = "Transaction failed" });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBill([FromBody] BillRequest request)
        {
            var bill = new Bill
            {
                UserId = UserId,
                Name = request.Description, // Maps DTO Description to your model Name
                Amount = request.Amount,
                NextDueDate = request.DueDate, // Maps DTO DueDate to your model NextDueDate
                CategoryId = request.CategoryId,
                DueDay = request.DueDate.Day, // Automatically sets DueDay from the date
                Frequency = request.Frequency ?? "Monthly",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            return Ok(bill);
        }
        [HttpPost("skip/{id}")]
        public async Task<IActionResult> SkipBill(int id)
        {
            var bill = await _context.Bills.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);

            if (bill == null) return NotFound(new { message = "Bill not found" });

            // Move the date forward 1 month WITHOUT creating a transaction or changing balance
            var oldDate = bill.NextDueDate;
            bill.NextDueDate = bill.NextDueDate.AddMonths(1);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Skipped payment for {oldDate:MMMM}. Next due date updated.",
                nextDueDate = bill.NextDueDate
            });
        }
    }
}
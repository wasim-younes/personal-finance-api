using thepiapi.Data;
using thepiapi.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace thepiapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var now = DateTime.UtcNow;
                var today = DateOnly.FromDateTime(now);
                var firstDayOfMonth = new DateOnly(now.Year, now.Month, 1);
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

                // 1. Fetch Accounts (Your Manual Wallets/Cards)
                var accounts = await _context.Accounts
                    .Where(a => a.UserId == UserId && (a.IsActive == null || a.IsActive == true))
                    .ToListAsync();

                // 2. Fetch Transactions for the Month
                var thisMonthTransactions = await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.UserId == UserId && t.TransactionDate >= firstDayOfMonth)
                    .ToListAsync();

                // 3. Fetch Budgets
                var budgets = await _context.Budgets
                    .Include(b => b.Category)
                    .Where(b => b.UserId == UserId && (b.IsActive == null || b.IsActive == true))
                    .ToListAsync();

                // 4. Fetch Bills (NextDueDate logic)
                var allActiveBills = await _context.Bills
                    .Include(b => b.Category)
                    .Where(b => b.UserId == UserId && (b.IsActive == null || b.IsActive == true))
                    .OrderBy(b => b.NextDueDate)
                    .ToListAsync();

                // 5. Calculate Category Spending (Pie Chart)
                var categorySpending = thisMonthTransactions
                    .Where(t => t.Amount < 0)
                    .GroupBy(t => new
                    {
                        t.CategoryId,
                        Name = t.Category?.Name ?? "General",
                        Color = t.Category?.Color ?? "#6B7280",
                        Icon = t.Category?.Icon ?? "tag"
                    })
                    .Select(g => new CategorySpending
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.Name,
                        CategoryColor = g.Key.Color,
                        CategoryIcon = g.Key.Icon,
                        Amount = Math.Abs(g.Sum(t => t.Amount)),
                        TransactionCount = g.Count()
                    }).ToList();

                var totalExpenses = categorySpending.Sum(c => c.Amount);
                foreach (var item in categorySpending)
                {
                    item.Percentage = totalExpenses > 0 ? (item.Amount / totalExpenses) * 100 : 0;
                }

                // 6. Build the Full Response
                var response = new DashboardResponse
                {
                    Summary = new FinancialSummary
                    {
                        TotalBalance = accounts.Sum(a => a.Balance ?? 0.0),
                        MonthlyIncome = thisMonthTransactions.Where(t => t.Amount > 0).Sum(t => t.Amount),
                        MonthlyExpenses = totalExpenses,
                        MonthlyNet = thisMonthTransactions.Sum(t => t.Amount),
                        TodaysSpending = Math.Abs(thisMonthTransactions.Where(t => t.TransactionDate == today && t.Amount < 0).Sum(t => t.Amount)),
                        ThisWeekSpending = Math.Abs(thisMonthTransactions.Where(t => t.TransactionDate >= startOfWeek && t.Amount < 0).Sum(t => t.Amount)),
                        ActiveBudgets = budgets.Count,
                        // Count bills that are due now or in the future
                        UpcomingBillsCount = allActiveBills.Count(b => b.NextDueDate >= DateTime.Today)
                    },

                    Accounts = accounts.Select(a => new AccountBalance
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Balance = a.Balance ?? 0.0,
                        Color = a.Color ?? "#3B82F6",
                        Icon = a.Icon ?? "wallet",
                        Type = a.Type ?? "Cash"
                    }).ToList(),

                    Budgets = budgets.Select(b =>
                    {
                        var spent = thisMonthTransactions.Where(t => t.CategoryId == b.CategoryId && t.Amount < 0).Sum(t => Math.Abs(t.Amount));
                        var percentage = b.Amount > 0 ? (spent / b.Amount) * 100 : 0;
                        return new BudgetProgress
                        {
                            Id = b.Id,
                            CategoryName = b.Category?.Name ?? "General",
                            BudgetAmount = b.Amount,
                            Spent = spent,
                            Remaining = Math.Max(0, b.Amount - spent),
                            Percentage = percentage,
                            Color = b.Category?.Color ?? "#6B7280",
                            Status = percentage >= 100 ? "Exceeded" : percentage >= 80 ? "Almost Reached" : "On Track"
                        };
                    }).ToList(),

                    // Updated Bills Section with "IsOverdue" control
                    UpcomingBills = allActiveBills.Select(b => new UpcomingBill
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Amount = b.Amount,
                        DueDate = b.NextDueDate,
                        DaysUntilDue = (b.NextDueDate.Date - DateTime.Today).Days,
                        CategoryName = b.Category?.Name ?? "General",
                        // This tells your UI to highlight the bill for manual confirmation
                        IsOverdue = b.NextDueDate.Date < DateTime.Today
                    }).ToList(),

                    RecentTransactions = thisMonthTransactions
                        .OrderByDescending(t => t.TransactionDate)
                        .Take(5)
                        .Select(t => new TransactionResponse
                        {
                            Id = t.Id,
                            Amount = t.Amount,
                            Description = t.Description,
                            CategoryName = t.Category?.Name ?? "General",
                            CategoryColor = t.Category?.Color ?? "#6B7280",
                            TransactionDate = t.TransactionDate
                        }).ToList(),

                    CategorySpending = categorySpending
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Dashboard Refresh Error", details = ex.Message });
            }
        }
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var startOfMonth = new DateOnly(today.Year, today.Month, 1);
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);

                var summary = await GetFinancialSummary(userId, today, startOfMonth, startOfWeek);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, new { message = "An error occurred while loading summary" });
            }
        }

        // GET: api/dashboard/accounts
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                var startOfMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

                var accounts = await GetAccountBalances(userId, startOfMonth);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard accounts");
                return StatusCode(500, new { message = "An error occurred while loading accounts" });
            }
        }

        // GET: api/dashboard/budgets
        [HttpGet("budgets")]
        public async Task<IActionResult> GetBudgets()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                var startOfMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

                var budgets = await GetBudgetProgress(userId, startOfMonth);
                return Ok(budgets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard budgets");
                return StatusCode(500, new { message = "An error occurred while loading budgets" });
            }
        }

        // GET: api/dashboard/bills/upcoming
        [HttpGet("bills/upcoming")]
        public async Task<IActionResult> GetUpcomingDashboardBills()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                var bills = await GetUpcomingBills(userId);
                return Ok(bills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming bills for dashboard");
                return StatusCode(500, new { message = "An error occurred while loading upcoming bills" });
            }
        }

        // GET: api/dashboard/categories/spending
        [HttpGet("categories/spending")]
        public async Task<IActionResult> GetCategorySpendingDashboard()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                var startOfMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

                var spending = await GetCategorySpending(userId, startOfMonth);
                return Ok(spending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category spending for dashboard");
                return StatusCode(500, new { message = "An error occurred while loading category spending" });
            }
        }

        // =========== PRIVATE HELPER METHODS ===========

        private async Task<FinancialSummary> GetFinancialSummary(int userId, DateOnly today, DateOnly startOfMonth, DateOnly startOfWeek)
        {
            // Total Balance (sum of all accounts that should be included)
            var totalBalance = await _context.Accounts
                .Where(a => a.UserId == userId &&
                           (a.IncludeInTotal == null || a.IncludeInTotal == true) &&
                           (a.IsActive == null || a.IsActive == true))
                .SumAsync(a => a.Balance ?? 0);

            // Monthly Income (positive transactions this month)
            var monthlyIncome = await _context.Transactions
                .Where(t => t.UserId == userId &&
                           t.Amount > 0 &&
                           t.TransactionDate >= startOfMonth)
                .SumAsync(t => t.Amount);

            // Monthly Expenses (negative transactions this month)
            var monthlyExpenses = await _context.Transactions
                .Where(t => t.UserId == userId &&
                           t.Amount < 0 &&
                           t.TransactionDate >= startOfMonth)
                .SumAsync(t => Math.Abs(t.Amount));

            // Today's Spending
            var todaysSpending = await _context.Transactions
                .Where(t => t.UserId == userId &&
                           t.Amount < 0 &&
                           t.TransactionDate == today)
                .SumAsync(t => Math.Abs(t.Amount));

            // This Week's Spending (Monday to today)
            var thisWeekSpending = await _context.Transactions
                .Where(t => t.UserId == userId &&
                           t.Amount < 0 &&
                           t.TransactionDate >= startOfWeek)
                .SumAsync(t => Math.Abs(t.Amount));

            // Active Budgets Count
            var activeBudgets = await _context.Budgets
                .CountAsync(b => b.UserId == userId &&
                                (b.IsActive == null || b.IsActive == true));

            // Upcoming Bills Count (next 7 days)
            var nextWeek = DateTime.UtcNow.AddDays(7);
            var upcomingBillsCount = await _context.Bills
                .CountAsync(b => b.UserId == userId &&
                                b.NextDueDate <= nextWeek &&
                                (b.IsActive == null || b.IsActive == true));

            return new FinancialSummary
            {
                TotalBalance = totalBalance,
                MonthlyIncome = monthlyIncome,
                MonthlyExpenses = monthlyExpenses,
                MonthlyNet = monthlyIncome - monthlyExpenses,
                TodaysSpending = todaysSpending,
                ThisWeekSpending = thisWeekSpending,
                ActiveBudgets = activeBudgets,
                UpcomingBillsCount = upcomingBillsCount
            };
        }

        private async Task<List<AccountBalance>> GetAccountBalances(int userId, DateOnly startOfMonth)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId &&
                           (a.IsActive == null || a.IsActive == true))
                .ToListAsync();

            var result = new List<AccountBalance>();

            foreach (var account in accounts)
            {
                // Calculate how much was spent/added this month on this account
                var thisMonthChange = await _context.Transactions
                    .Where(t => t.UserId == userId &&
                               t.AccountId == account.Id &&
                               t.TransactionDate >= startOfMonth)
                    .SumAsync(t => t.Amount);

                result.Add(new AccountBalance
                {
                    Id = account.Id,
                    Name = account.Name,
                    Type = account.Type ?? "Unknown",
                    Balance = account.Balance ?? 0,
                    Color = account.Color ?? "#6B7280",
                    Icon = account.Icon ?? "bank",
                    ThisMonthChange = thisMonthChange
                });
            }

            return result;
        }

        private async Task<List<BudgetProgress>> GetBudgetProgress(int userId, DateOnly startOfMonth)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId &&
                           (b.IsActive == null || b.IsActive == true) &&
                           b.StartDate <= DateTime.UtcNow)
                .ToListAsync();

            var result = new List<BudgetProgress>();

            foreach (var budget in budgets)
            {
                // Calculate how much was spent in this category this month
                double spent = 0;

                if (budget.CategoryId.HasValue)
                {
                    // Category-specific budget
                    spent = Math.Abs(await _context.Transactions
                        .Where(t => t.UserId == userId &&
                                   t.CategoryId == budget.CategoryId.Value &&
                                   t.Amount < 0 && // Only expenses
                                   t.TransactionDate >= startOfMonth)
                        .SumAsync(t => t.Amount));
                }
                else
                {
                    // Total budget (all expenses)
                    spent = Math.Abs(await _context.Transactions
                        .Where(t => t.UserId == userId &&
                                   t.Amount < 0 && // Only expenses
                                   t.TransactionDate >= startOfMonth)
                        .SumAsync(t => t.Amount));
                }

                var remaining = budget.Amount - spent;
                var percentage = budget.Amount > 0 ? (spent / budget.Amount) * 100 : 0;

                string status = "On Track";
                if (percentage >= 100) status = "Exceeded";
                else if (percentage >= 80) status = "Almost Reached";

                result.Add(new BudgetProgress
                {
                    Id = budget.Id,
                    CategoryName = budget.Category?.Name ?? "Total Budget",
                    BudgetAmount = budget.Amount,
                    Spent = spent,
                    Remaining = remaining,
                    Percentage = percentage,
                    Status = status,
                    Color = budget.Category?.Color ?? "#3B82F6"
                });
            }

            return result;
        }

        private async Task<List<TransactionResponse>> GetRecentTransactions(int userId, int count)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.CreatedDate)
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
                    PaymentMethod = t.PaymentMethod
                })
                .ToListAsync();

            return transactions;
        }

        private async Task<List<UpcomingBill>> GetUpcomingBills(int userId)
        {
            var nextWeek = DateTime.UtcNow.AddDays(7);

            var bills = await _context.Bills
                .Include(b => b.Category)
                .Where(b => b.UserId == userId &&
                           b.NextDueDate <= nextWeek &&
                           (b.IsActive == null || b.IsActive == true))
                .OrderBy(b => b.NextDueDate)
                .Select(b => new UpcomingBill
                {
                    Id = b.Id,
                    Name = b.Name,
                    Amount = b.Amount,
                    DueDate = b.NextDueDate,
                    DaysUntilDue = (int)(b.NextDueDate - DateTime.UtcNow).TotalDays,
                    CategoryName = b.Category.Name
                })
                .ToListAsync();

            return bills;
        }

        private async Task<List<CategorySpending>> GetCategorySpending(int userId, DateOnly startOfMonth)
        {
            var spending = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId &&
                           t.Amount < 0 && // Only expenses
                           t.TransactionDate >= startOfMonth)
                .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color, t.Category.Icon })
                .Select(g => new CategorySpending
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    CategoryColor = g.Key.Color ?? "#6B7280",
                    CategoryIcon = g.Key.Icon ?? "tag",
                    Amount = Math.Abs(g.Sum(t => t.Amount)),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(c => c.Amount)
                .Take(8) // Top 8 categories
                .ToListAsync();

            // Calculate percentages
            var totalExpenses = spending.Sum(c => c.Amount);
            if (totalExpenses > 0)
            {
                foreach (var category in spending)
                {
                    category.Percentage = (category.Amount / totalExpenses) * 100;
                }
            }

            return spending;
        }
    }
}
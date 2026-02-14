using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using thepiapi.Data;
using thepiapi.Models;
using thepiapi.Models.DTOs;


namespace thepiapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public BudgetsController(ApplicationDbContext context) => _context = context;

        [HttpGet("list")]
        public async Task<IActionResult> GetBudgets()
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateOnly(now.Year, now.Month, 1);

            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == UserId)
                .ToListAsync();

            var response = new List<BudgetResponse>();

            foreach (var budget in budgets)
            {
                // Calculate how much was spent in this category this month
                var spent = await _context.Transactions
                    .Where(t => t.UserId == UserId &&
                                t.CategoryId == budget.CategoryId &&
                                t.TransactionDate >= firstDayOfMonth &&
                                t.Amount < 0)
                    .SumAsync(t => Math.Abs(t.Amount));

                response.Add(new BudgetResponse
                {
                    Id = budget.Id,
                    // Using ?? 0 handles the CS0266 error and CS8629 warning
                    CategoryId = budget.CategoryId ?? 0,
                    CategoryName = budget.Category?.Name ?? "General",
                    CategoryColor = budget.Category?.Color ?? "#6B7280",
                    LimitAmount = budget.Amount,
                    SpentAmount = spent,
                    RemainingAmount = Math.Max(0, budget.Amount - spent),
                    ProgressPercentage = budget.Amount > 0 ? (spent / budget.Amount) * 100 : 0
                });
            }

            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBudget([FromBody] BudgetRequest request)
        {
            var budget = new Budget
            {
                UserId = UserId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Period = request.Period,
                StartDate = request.StartDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return Ok(budget);
        }
    }
}

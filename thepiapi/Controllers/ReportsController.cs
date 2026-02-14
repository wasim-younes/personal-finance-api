using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using thepiapi.Data;
using thepiapi.Models.DTOs;
using System.Globalization;

namespace thepiapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context) => _context = context;

        [HttpGet("monthly-trends")]
        public async Task<IActionResult> GetMonthlyTrends()
        {
            // Look back 6 months
            var sixMonthsAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6));

            var transactions = await _context.Transactions
                .Where(t => t.UserId == UserId && t.TransactionDate >= sixMonthsAgo)
                .ToListAsync();

            var report = transactions
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new MonthlyTrend
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key.Month),
                    Income = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                    Expenses = Math.Abs(g.Where(t => t.Amount < 0).Sum(t => t.Amount))
                })
                .ToList();

            return Ok(report);
        }

        [HttpGet("spending-by-category")]
        public async Task<IActionResult> GetCategorySpending([FromQuery] int month, [FromQuery] int year)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == UserId &&
                            t.TransactionDate.Month == month &&
                            t.TransactionDate.Year == year &&
                            t.Amount < 0) // Only expenses
                .ToListAsync();

            var report = transactions
                .GroupBy(t => t.Category?.Name ?? "Uncategorized")
                .Select(g => new CategoryReport
                {
                    CategoryName = g.Key,
                    Amount = Math.Abs(g.Sum(t => t.Amount)),
                    Color = g.First().Category?.Color ?? "#6B7280"
                })
                .OrderByDescending(r => r.Amount)
                .ToList();

            return Ok(report);
        }
    }
}
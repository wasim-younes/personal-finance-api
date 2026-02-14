using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using thepiapi.Data;
using thepiapi.Models;
using thepiapi.Models.DTOs;

namespace thepiapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : BaseController
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. GET: api/categories/list
    [HttpGet("list")]
    public async Task<IActionResult> GetCategories()
    {
        // Get global categories (UserId is null) AND the user's private categories
        var categories = await _context.Categories
            .Where(c => c.UserId == null || c.UserId == UserId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories);
    }

    // 2. POST: api/categories/create
    [HttpPost("create")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Category name is required.");

        var category = new Category
        {
            Name = request.Name,
            Color = request.Color ?? "#6B7280",
            Icon = request.Icon ?? "tag",
            Type = request.Type ?? "Expense",
            UserId = UserId // Automatically assigned from your Token
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(category);
    }

    // 3. DELETE: api/categories/delete/{id}
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        // Users can only delete categories they created themselves
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);

        if (category == null)
            return NotFound("Category not found or you don't have permission to delete system categories.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Category deleted successfully" });
    }
    [HttpGet("stats")]
    public async Task<IActionResult> GetCategoryStats()
    {
        var stats = await _context.Categories
            .Where(c => c.UserId == UserId)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Color,
                TransactionCount = _context.Transactions.Count(t => t.CategoryId == c.Id),
                TotalSpent = Math.Abs(_context.Transactions.Where(t => t.CategoryId == c.Id && t.Amount < 0).Sum(t => t.Amount))
            })
            .OrderByDescending(x => x.TotalSpent)
            .ToListAsync();

        return Ok(stats);
    }

}
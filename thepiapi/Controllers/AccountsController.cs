using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using thepiapi.Data;
using thepiapi.Models;
using thepiapi.Models.DTOs;

namespace thepiapi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AccountsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public AccountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. POST: api/accounts/create
    [HttpPost("create")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var account = new Account
        {
            UserId = userId,
            Name = request.Name,
            Type = request.Type,
            Balance = request.Balance,
            Currency = request.Currency ?? "USD",
            Color = request.Color ?? "#3B82F6",
            Icon = request.Icon ?? "wallet",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return Ok(new AccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            Balance = account.Balance ?? 0.0,
            Currency = account.Currency,
            Color = account.Color,
            Icon = account.Icon
        });
    }

    // 2. GET: api/accounts/list
    [HttpGet("list")]
    public async Task<IActionResult> GetMyAccounts()
    {
        var userId = GetUserId();

        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId && (a.IsActive == null || a.IsActive == true))
            .Select(a => new AccountResponse
            {
                Id = a.Id,
                Name = a.Name,
                Type = a.Type,
                Balance = a.Balance ?? 0.0,
                Currency = a.Currency ?? "USD",
                Color = a.Color,
                Icon = a.Icon
            })
            .ToListAsync();

        return Ok(accounts);
    }

    // 3. GET: api/accounts/details/{id}
    [HttpGet("details/{id}")]
    public async Task<IActionResult> GetAccount(int id)
    {
        var userId = GetUserId();
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (account == null) return NotFound(new { message = "Account not found" });

        return Ok(new AccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            Balance = account.Balance ?? 0.0,
            Currency = account.Currency ?? "USD",
            Color = account.Color,
            Icon = account.Icon
        });
    }

    // 4. DELETE: api/accounts/delete/{id}
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var userId = GetUserId();
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (account == null) return NotFound();

        // Soft delete logic
        account.IsActive = false;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Account deactivated successfully" });
    }
    [HttpPost("{id}/reconcile")]
    public async Task<IActionResult> ReconcileAccount(int id, [FromBody] ReconcileRequest request)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Verify the Account belongs to THIS user
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

            if (account == null)
                return NotFound(new { message = $"Account {id} not found for User {UserId}" });

            double currentBalance = account.Balance ?? 0;
            double difference = request.ActualBalance - currentBalance;

            if (Math.Abs(difference) < 0.01)
                return Ok(new { message = "Balances match.", balance = account.Balance });

            // 2. Build the Transaction with EXPLICIT IDs
            var adjustment = new thepiapi.Models.Transaction
            {
                UserId = UserId,      // Must exist in Users table
                AccountId = id,       // Must exist in Accounts table
                CategoryId = 9,       // Must exist in Categories table
                Amount = difference,
                Description = "Balance Adjustment (Manual Sync)",
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedDate = DateTime.UtcNow,
                Notes = $"Adjusted from {currentBalance} to {request.ActualBalance}",

                // CRITICAL: Set navigation properties to null 
                // This prevents EF from trying to insert new related records
                User = null!,
                Account = null!,
                Category = null!
            };

            account.Balance = request.ActualBalance;

            _context.Transactions.Add(adjustment);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return Ok(new { newBalance = account.Balance, adjustment = difference });
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            // This will now print the exact IDs we tried to use in the console
            Console.WriteLine($"FK Fail Check - User: {UserId}, Account: {id}, Category: 9");
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }
}
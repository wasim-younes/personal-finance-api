using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using thepiapi.Data;
using thepiapi.Models;
using thepiapi.Models.DTOs;

namespace thepiapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransfersController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public TransfersController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public async Task<IActionResult> TransferMoney([FromBody] TransferRequest request)
        {
            if (request.FromAccountId == request.ToAccountId)
                return BadRequest(new { message = "Source and destination accounts must be different." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var fromAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == request.FromAccountId && a.UserId == UserId);

                var toAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == request.ToAccountId && a.UserId == UserId);
                if (fromAccount == null || toAccount == null)
                    return NotFound(new { message = "One or both accounts not found." });

                if ((fromAccount.Balance ?? 0) < request.Amount)
                    return BadRequest(new { message = "Insufficient funds in source account." });

                // 1. Subtract from source
                fromAccount.Balance -= request.Amount;

                // 2. Add to destination
                toAccount.Balance = (toAccount.Balance ?? 0) + request.Amount;

                // 3. Optional: Log these as special transactions or in a separate Transfers table
                // For a manual ledger, we'll just update the balances for now.

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Transfer successful",
                    fromBalance = fromAccount.Balance,
                    toBalance = toAccount.Balance
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Transfer failed", details = ex.Message });
            }
        }
    }
}
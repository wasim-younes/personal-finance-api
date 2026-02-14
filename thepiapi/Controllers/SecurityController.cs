using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using thepiapi.Data;
using thepiapi.Models.DTOs;
using BCrypt.Net;

namespace thepiapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public SecurityController(ApplicationDbContext context) => _context = context;

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] UserPasswordUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user == null) return NotFound();

            // 1. Verify old password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Current password is incorrect." });
            }

            // 2. Hash and save new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password updated successfully." });
        }

        [HttpDelete("delete-my-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            // The "Nuclear Option" for your manual ledger
            var user = await _context.Users.FindAsync(UserId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "All your data has been permanently deleted." });
        }
        [HttpPost("set-pin")]
        public async Task<IActionResult> SetPin([FromBody] SetPinRequest request)
        {
            if (request.Pin.Length < 4 || request.Pin.Length > 6)
                return BadRequest(new { message = "PIN must be between 4 and 6 digits." });

            var user = await _context.Users.FindAsync(UserId);
            if (user == null) return NotFound();

            // Hash the PIN before saving
            user.AppPinHash = BCrypt.Net.BCrypt.HashPassword(request.Pin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mobile PIN set successfully." });
        }
        [HttpPost("verify-pin")]
        public async Task<IActionResult> VerifyPin([FromBody] SetPinRequest request)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(user.AppPinHash))
            {
                return BadRequest(new { message = "No PIN has been set for this account." });
            }

            // Verify the hashed PIN
            bool isValid = BCrypt.Net.BCrypt.Verify(request.Pin, user.AppPinHash);

            if (!isValid)
            {
                return Unauthorized(new { message = "Invalid PIN." });
            }

            return Ok(new { message = "PIN verified. Access granted." });
        }
    }
}
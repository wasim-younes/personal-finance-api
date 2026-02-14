using thepiapi.Data;
using thepiapi.Helpers;
using thepiapi.Models.DTOs;
using thepiapi.Models;
using thepiapi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace thepiapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // 1. Create the database entity (User.cs)
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                MonthlyIncome = request.MonthlyIncome,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                Currency = "USD"
            };

            // 2. Save to DB to get the Real ID
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 3. Generate Token
            var token = _jwtService.GenerateToken(user);

            // 4. Return AuthResponse (Matches your DTO exactly now)
            return Ok(new AuthResponse
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(7),
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Currency = user.Currency ?? "USD",
                    MonthlyIncome = user.MonthlyIncome ?? 0.0,
                    CreatedAt = user.CreatedAt ?? DateTime.UtcNow
                }
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password" });

            if (user.IsActive == false)
                return Unauthorized(new { message = "Account is deactivated" });

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate token
            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(7),
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName ?? "",
                    Currency = user.Currency ?? "USD",
                    MonthlyIncome = user.MonthlyIncome ?? 0, // Fixed: no cast needed
                    CreatedAt = user.CreatedAt ?? DateTime.UtcNow
                }
            });
        }
    }
}
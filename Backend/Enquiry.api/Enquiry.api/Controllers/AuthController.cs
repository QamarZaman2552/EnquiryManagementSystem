using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Enquiry.api.Models;

namespace Enquiry.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EnquiryDbContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public AuthController(EnquiryDbContext context, IConfiguration config, IWebHostEnvironment env)
        {
            _context = context;
            _config = config;
            _env = env;
        }

        [HttpPost("login")]
        [EnableRateLimiting("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                return Unauthorized(new { message = "Account locked. Try again later." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                    user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "Invalid username or password" });
            }

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync();

            var (accessToken, expiresAt) = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            SetTokenCookie("access_token", accessToken, expiresAt);
            SetTokenCookie("refresh_token", refreshToken, DateTime.UtcNow.AddDays(7));

            return Ok(new { username = user.Username, role = user.Role });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var rawRefreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(rawRefreshToken))
                return Unauthorized(new { message = "No refresh token" });

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == rawRefreshToken && !rt.IsRevoked);

            if (storedToken == null || storedToken.User == null || storedToken.Expires < DateTime.UtcNow)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            storedToken.IsRevoked = true;
            var user = storedToken.User;

            var (newAccessToken, expiresAt) = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            SetTokenCookie("access_token", newAccessToken, expiresAt);
            SetTokenCookie("refresh_token", newRefreshToken, DateTime.UtcNow.AddDays(7));

            return Ok(new { username = user.Username, role = user.Role });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var rawRefreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(rawRefreshToken))
            {
                var storedTokens = await _context.RefreshTokens
                    .Where(rt => rt.Token == rawRefreshToken)
                    .ToListAsync();
                foreach (var t in storedTokens)
                    t.IsRevoked = true;
                await _context.SaveChangesAsync();
            }

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            return Ok(new { message = "Logged out" });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role);
            return Ok(new { username, role });
        }

        private (string token, DateTime expiresAt) GenerateJwtToken(User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                ?? _config["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT_KEY is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userId", user.UserId.ToString())
            };

            var expiresAt = DateTime.UtcNow.AddHours(8);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private void SetTokenCookie(string name, string value, DateTime expires)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
                Expires = expires,
                Path = "/"
            };
            Response.Cookies.Append(name, value, options);
        }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}

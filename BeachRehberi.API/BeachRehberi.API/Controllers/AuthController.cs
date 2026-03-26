using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs;
using BCrypt.Net;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly BeachDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(BeachDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.BusinessUsers
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(ApiResponse<string>.FailureResult("Geçersiz e-posta veya şifre."));
            }

            var token = GenerateJwtToken(user);
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResult(new {
                Token = token,
                User = new { user.Id, user.Email, user.ContactName, user.Role, user.BeachId }
            }));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.BusinessUsers.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest(ApiResponse<string>.FailureResult("Bu e-posta adresi zaten kullanımda."));
            }

            // BCrypt Hashing (WorkFactor 11)
            var newUser = new BusinessUser
            {
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, 11),
                ContactName = registerDto.ContactName,
                BeachId = registerDto.BeachId,
                Role = "BusinessOwner",
                IsActive = true
            };

            _context.BusinessUsers.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResult(null, "Hesap başarıyla oluşturuldu."));
        }

        private string GenerateJwtToken(BusinessUser user)
        {
            var secret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET") 
                         ?? _config["Jwt:SecretKey"] 
                         ?? "Development_Secret_Key_Do_Not_Use_In_Production_2026!";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("BeachId", user.BeachId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "BeachRehberi.API",
                audience: _config["Jwt:Audience"] ?? "BeachRehberi.App",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

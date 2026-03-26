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

        // ─── LOGIN ENDPOINT ───────────────────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Sadece Aktif kullanıcıları ve Email ile bul
            var user = await _context.BusinessUsers
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

            // BCrypt Doğrulama: Güvenlik için en sağlam yöntem
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(ApiResponse<string>.FailureResult("E-posta veya şifre hatalı."));
            }

            var token = GenerateJwtToken(user);
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResult(new
            {
                Token = token,
                User = new { user.Id, user.Email, user.ContactName, user.Role, user.BeachId }
            }, "Giriş başarılı."));
        }

        // ─── REGISTER ENDPOINT (Gerçek API Bağlantısı İçin) ────────
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Validasyon: Email benzersiz olmalı
            if (await _context.BusinessUsers.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest(ApiResponse<string>.FailureResult("Bu e-posta adresi zaten kayıtlı."));
            }

            // BCrypt Hashing: Şifreyi güvenli hale getir
            string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(registerDto.Password, 11);

            var newUser = new BusinessUser
            {
                Email = registerDto.Email,
                PasswordHash = hashedPassword,
                ContactName = registerDto.ContactName,
                BeachId = registerDto.BeachId,
                Role = "BusinessOwner", // Kayıt olanlar işletmeci olarak başlar
                IsActive = true
            };

            _context.BusinessUsers.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResult(null, "Hesabınız başarıyla oluşturuldu."));
        }

        private string GenerateJwtToken(BusinessUser user)
        {
            // Environment variable'dan oku (Program.cs ile paralel)
            var secretKeyString = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET") 
                                ?? _config["Jwt:SecretKey"] 
                                ?? "BeachRehberi_Development_Only_Super_Secret_Key_2026!";
            
            var key = Encoding.ASCII.GetBytes(secretKeyString);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("BeachId", user.BeachId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Token 7 gün geçerli
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"] ?? "BeachRehberi.API",
                Audience = _config["Jwt:Audience"] ?? "BeachRehberi.App"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
    public class RegisterDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; public string ContactName { get; set; } = ""; public int BeachId { get; set; } }
}

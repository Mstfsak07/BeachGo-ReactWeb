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
                .Include(u => u.Beach)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.BusinessUsers.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest(ApiResponse<string>.FailureResult("Bu e-posta adresi zaten kullanımda."));
            }

            var newUser = new BusinessUser
            {
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                ContactName = registerDto.ContactName,
                BeachId = registerDto.BeachId,
                Role = "BusinessOwner"
            };

            _context.BusinessUsers.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResult(null, "Kayıt başvurunuz başarıyla alındı."));
        }

        private string GenerateJwtToken(BusinessUser user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);
            
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
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
    public class RegisterDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; public string ContactName { get; set; } = ""; public int BeachId { get; set; } }
}

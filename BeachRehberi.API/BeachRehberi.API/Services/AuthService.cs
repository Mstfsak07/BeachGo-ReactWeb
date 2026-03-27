using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace BeachRehberi.API.Services;

public class AuthService : IAuthService {
    private readonly BeachDbContext _db;
    public AuthService(BeachDbContext db) { _db = db; }

    public async Task<AuthResponse?> LoginAsync(string email, string password) {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        return new AuthResponse {
            Email = user.Email,
            Token = GenerateJwtToken(user),
            Role = user.Role
        };
    }

    public async Task<BusinessUser?> RegisterAsync(RegisterRequest request) {
        if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email)) return null;

        var user = new BusinessUser {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            BusinessName = request.BusinessName,
            BeachId = request.BeachId,
            CreatedAt = DateTime.UtcNow
        };
        _db.BusinessUsers.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    private string GenerateJwtToken(BusinessUser user) {
        var jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET");
        if (string.IsNullOrEmpty(jwtSecret))
            throw new InvalidOperationException("BEACHGO_JWT_SECRET is missing!");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("BeachId", user.BeachId.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = "BeachRehberi.API",
            Audience = "BeachRehberi.App",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BeachRehberi.API.Services;

public class AuthService : IAuthService
{
    private readonly BeachDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(BeachDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _db.BusinessUsers
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null)
        {
            Console.WriteLine($"KULLANICI BULUNAMADI: {email}");
            return null;
        }

        Console.WriteLine($"KULLANICI BULUNDU: {user.Email}");

        var isValid = VerifyPassword(password, user.PasswordHash);
        Console.WriteLine($"VALID: {isValid}");

        if (!isValid) return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return GenerateToken(user);
    }

    public async Task<BusinessUser?> GetUserAsync(string email) =>
        await _db.BusinessUsers
            .Include(u => u.Beach)
            .FirstOrDefaultAsync(u => u.Email == email);

    private string GenerateToken(BusinessUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("BeachId", user.BeachId.ToString()),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, "Business")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
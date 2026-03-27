using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BeachRehberi.API.Services;

public class TokenService : ITokenService
{
    private readonly BeachDbContext _db;
    public TokenService(BeachDbContext db) { _db = db; }

    public string GenerateAccessToken(BusinessUser user)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET");
        if (string.IsNullOrEmpty(jwtSecret))
            throw new InvalidOperationException("BEACHGO_JWT_SECRET is missing!");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("BeachId", user.BeachId.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = "BeachRehberi.API",
            Audience = "BeachRehberi.App",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task BlacklistTokenAsync(string token, DateTime expiry)
    {
        if (await _db.RevokedTokens.AnyAsync(r => r.Token == token)) return;
        _db.RevokedTokens.Add(new RevokedToken { Token = token, ExpiryDate = expiry });
        await _db.SaveChangesAsync();
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        return await _db.RevokedTokens.AnyAsync(r => r.Token == token);
    }
}


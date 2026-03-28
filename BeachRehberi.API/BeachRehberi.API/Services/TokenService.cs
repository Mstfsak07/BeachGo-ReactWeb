using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace BeachRehberi.API.Services;

public class TokenService : ITokenService
{
    private readonly BeachDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenService> _logger;
    private readonly string _jwtSecret;
    private const string BlacklistCachePrefix = "bl_";

    public TokenService(BeachDbContext db, IMemoryCache cache, IConfiguration configuration, ILogger<TokenService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
        _jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET") 
                     ?? configuration["Jwt:SecretKey"] 
                     ?? throw new InvalidOperationException("JWT Secret is missing!");
    }

    public string GenerateAccessToken(BusinessUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        if (user.BeachId.HasValue)
        {
            claims.Add(new Claim("BeachId", user.BeachId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
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
        _logger.LogInformation("Blacklisting token expiring at {Expiry}", expiry);
        
        if (!await _db.RevokedTokens.AnyAsync(r => r.Token == token))
        {
            _db.RevokedTokens.Add(new RevokedToken { Token = token, ExpiryDate = expiry });
            await _db.SaveChangesAsync();
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiry);
        _cache.Set(BlacklistCachePrefix + token, true, cacheEntryOptions);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        if (_cache.TryGetValue(BlacklistCachePrefix + token, out bool isBlacklisted))
        {
            return isBlacklisted;
        }

        var inDb = await _db.RevokedTokens.AnyAsync(r => r.Token == token);
        if (inDb)
        {
            _cache.Set(BlacklistCachePrefix + token, true, TimeSpan.FromMinutes(15));
        }

        return inDb;
    }
}

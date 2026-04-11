using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace BeachRehberi.API.Services;

public class TokenService : ITokenService
{
    private readonly BeachDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenService> _logger;
    private readonly string _jwtSecret;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;
    private const string BlacklistCachePrefix = "bl_";

    public TokenService(BeachDbContext db, IMemoryCache cache, IConfiguration configuration, ILogger<TokenService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
        _jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET")
                     ?? configuration["JwtSettings:SecretKey"]
                     ?? configuration["Jwt:SecretKey"]
                     ?? throw new InvalidOperationException("JWT Secret is missing!");
        
        var jwtSettings = configuration.GetSection("JwtSettings");
        _accessTokenExpiryMinutes = jwtSettings.GetValue<int?>("AccessTokenExpiryMinutes") ?? configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
        _refreshTokenExpiryDays = jwtSettings.GetValue<int?>("RefreshTokenExpiryDays") ?? configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);
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
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
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

    public async Task RevokeAccessToken(string jti)
    {
        if (string.IsNullOrEmpty(jti)) return;

        var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes);
        var cacheDuration = expiresAt - DateTime.UtcNow;

        if (!await _db.RevokedTokens.AnyAsync(r => r.Token == jti))
        {
            _db.RevokedTokens.Add(new RevokedToken
            {
                Token = jti,
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            });
            await _db.SaveChangesAsync();
        }

        if (cacheDuration > TimeSpan.Zero)
        {
            _cache.Set(BlacklistCachePrefix + jti, true, cacheDuration);
        }
    }

    public async Task<bool> IsTokenRevoked(string jti)
    {
        if (string.IsNullOrEmpty(jti)) return true;

        if (_cache.TryGetValue(BlacklistCachePrefix + jti, out bool isRevoked))
        {
            return isRevoked;
        }

        var inDb = await _db.RevokedTokens.AnyAsync(r => r.Token == jti);
        if (inDb)
        {
            _cache.Set(BlacklistCachePrefix + jti, true, TimeSpan.FromMinutes(_accessTokenExpiryMinutes));
        }

        return inDb;
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var hashedToken = RefreshToken.HashToken(refreshToken);
        var tokenEntity = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken);

        if (tokenEntity == null || !tokenEntity.IsActive)
            return AuthResult.Failure("Geçersiz veya süresi dolmuş refresh token.");

        var user = await _db.BusinessUsers.FindAsync(tokenEntity.UserId);
        if (user == null)
            return AuthResult.Failure("Kullanıcı bulunamadı.");

        // Rotate: Eski token'ı revoke et
        var newRefreshToken = GenerateRefreshToken();
        tokenEntity.RevokeAndReplace(newRefreshToken);

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshTokenEntity = new RefreshToken(
            user.Id, 
            newRefreshToken, 
            DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            tokenEntity.CreatedByIp,
            tokenEntity.CreatedByUserAgent);

        _db.RefreshTokens.Add(newRefreshTokenEntity);
        await _db.SaveChangesAsync();

        return AuthResult.SuccessResult(user, newAccessToken, newRefreshToken);
    }

    public async Task RevokeRefreshToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return;

        var hashedToken = RefreshToken.HashToken(token);
        var tokenEntity = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken);
        if (tokenEntity != null)
        {
            tokenEntity.Revoke("manual_revoke");
            await _db.SaveChangesAsync();
        }
    }

    public async Task RevokeAccessTokenAsync(string token)
    {
        var tokenMetadata = GetTokenMetadata(token);
        if (tokenMetadata != null)
        {
            await RevokeAccessToken(tokenMetadata.Value.Jti, tokenMetadata.Value.ExpiresAt);
        }
    }

    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        var jti = GetJtiFromToken(token);
        return string.IsNullOrEmpty(jti) || await IsTokenRevoked(jti);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken) => await RevokeRefreshToken(refreshToken);

    // Compatibility methods
    public async Task BlacklistTokenAsync(string token, DateTime expiry)
    {
        var tokenMetadata = GetTokenMetadata(token);
        if (tokenMetadata != null)
        {
            await RevokeAccessToken(tokenMetadata.Value.Jti, expiry.ToUniversalTime());
        }
    }
    public async Task<bool> IsTokenBlacklistedAsync(string token) => await IsTokenRevokedAsync(token);

    public async Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken)
    {
        var hashedToken = RefreshToken.HashToken(refreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken && rt.UserId == userId);
        return token != null && token.IsActive;
    }

    public async Task RevokeRefreshTokenAsync(int userId, string refreshToken)
    {
        var hashedToken = RefreshToken.HashToken(refreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken && rt.UserId == userId);
        if (token != null)
        {
            token.Revoke("manual_revoke");
            await _db.SaveChangesAsync();
        }
    }

    public async Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
    {
        var tokenEntity = new RefreshToken(userId, refreshToken, expiry, "unknown", "unknown");
        _db.RefreshTokens.Add(tokenEntity);
        await _db.SaveChangesAsync();
    }

    public ClaimsPrincipalResult? ValidateExpiredAccessToken(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            var principal = handler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = "BeachRehberi.API",
                ValidAudience = "BeachRehberi.App",
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase))
                return null;

            return new ClaimsPrincipalResult
            {
                UserId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                Email = principal.FindFirstValue(JwtRegisteredClaimNames.Email) ?? "",
                Role = principal.FindFirstValue(ClaimTypes.Role) ?? "",
                Jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti) ?? ""
            };
        }
        catch { return null; }
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
            ValidateLifetime = false,
            ValidIssuer = "BeachRehberi.API",
            ValidAudience = "BeachRehberi.App"
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private async Task RevokeAccessToken(string jti, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return;

        var normalizedExpiry = expiresAt.Kind == DateTimeKind.Utc ? expiresAt : expiresAt.ToUniversalTime();
        var cacheDuration = normalizedExpiry - DateTime.UtcNow;

        if (!await _db.RevokedTokens.AnyAsync(r => r.Token == jti))
        {
            _db.RevokedTokens.Add(new RevokedToken
            {
                Token = jti,
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = normalizedExpiry
            });
            await _db.SaveChangesAsync();
        }

        if (cacheDuration > TimeSpan.Zero)
        {
            _cache.Set(BlacklistCachePrefix + jti, true, cacheDuration);
        }
    }

    private (string Jti, DateTime ExpiresAt)? GetTokenMetadata(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return null;

        var jwt = handler.ReadJwtToken(token);
        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrWhiteSpace(jti))
            return null;

        var expiresAt = jwt.ValidTo == DateTime.MinValue
            ? DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes)
            : jwt.ValidTo.ToUniversalTime();

        return (jti, expiresAt);
    }

    private string? GetJtiFromToken(string token)
    {
        return GetTokenMetadata(token)?.Jti;
    }
}

using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BeachRehberi.API.Services;

public class AuthService : IAuthService
{
    private readonly BeachDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(BeachDbContext db, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            Email = user.Email,
            Token = accessToken,
            RefreshToken = refreshTokenStr,
            Role = user.Role
        };
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshTokenStr)
    {
        var affectedRows = await _db.RefreshTokens
            .Where(rt => rt.Token == refreshTokenStr && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true));

        if (affectedRows == 0) return null;

        var refreshToken = await _db.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(rt => rt.Token == refreshTokenStr);
        if (refreshToken == null) return null;

        var user = await _db.BusinessUsers.FindAsync(refreshToken.UserId);
        if (user == null) return null;

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenStr = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenStr,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            Email = user.Email,
            Token = newAccessToken,
            RefreshToken = newRefreshTokenStr,
            Role = user.Role
        };
    }

    public async Task LogoutAsync(string? accessTokenStr, string? refreshTokenStr)
    {
        int? userId = null;

        if (!string.IsNullOrEmpty(accessTokenStr))
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(accessTokenStr);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
                if (int.TryParse(userIdClaim, out int id)) userId = id;

                await _tokenService.BlacklistTokenAsync(accessTokenStr, jwtToken.ValidTo);
            }
            catch { /* Token invalid or expired, ignore */ }
        }

        if (!string.IsNullOrEmpty(refreshTokenStr))
        {
            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshTokenStr);
            if (rt != null)
            {
                rt.IsRevoked = true;
                userId ??= rt.UserId;
            }
        }

        if (userId.HasValue)
        {
            await _db.RefreshTokens
                .Where(rt => rt.UserId == userId.Value && !rt.IsRevoked)
                .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true));
        }

        await _db.SaveChangesAsync();
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email))
        {
            return RegisterResult.Failure("Bu e-posta adresi zaten kayıtlı.");
        }

        int? beachId = request.BeachId;
        if (beachId == 0) beachId = null;

        if (beachId.HasValue)
        {
            var beachExists = await _db.Beaches.AnyAsync(b => b.Id == beachId.Value);
            if (!beachExists)
            {
                return RegisterResult.Failure($"Seçilen plaj bulunamadı: {beachId.Value}.");
            }
        }

        var user = new BusinessUser
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            BusinessName = request.BusinessName,
            BeachId = beachId,
            ContactName = request.ContactName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Role = "BusinessOwner"
        };

        _db.BusinessUsers.Add(user);
        await _db.SaveChangesAsync();

        return RegisterResult.SuccessResult(user, "Kayıt başarılı.");
    }
}

public sealed record RegisterResult(bool Success, string Message, BusinessUser? User = null)
{
    public static RegisterResult Failure(string message) => new(false, message);
    public static RegisterResult SuccessResult(BusinessUser user, string message = "") => new(true, message, user);
}

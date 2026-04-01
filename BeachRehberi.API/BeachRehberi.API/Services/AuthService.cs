using System.Data;
using System.Transactions;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public async Task<ServiceResult<AuthResponse>> LoginAsync(string email, string password, string ipAddress, string userAgent)
    {
        var user = await _db.BusinessUsers
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null)
            return ServiceResult<AuthResponse>.FailureResult("Geçersiz kullanıcı bilgileri.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return ServiceResult<AuthResponse>.FailureResult("Geçersiz kullanıcı bilgileri.");

        // Önceki tüm refresh token'ları revoke et (tek oturum politikası)
        await InvalidateAllSessionsAsync(user.Id, "new_login");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken(
            user.Id, refreshTokenStr,
            DateTime.UtcNow.AddDays(7), ipAddress, userAgent));

        await _db.SaveChangesAsync();

        return ServiceResult<AuthResponse>.SuccessResult(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenStr,
            Email = user.Email,
            Role = user.Role,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15)
        }, "Giriş başarılı.");
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, string ipAddress, string userAgent)
    {
        if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email))
            return ServiceResult<AuthResponse>.FailureResult("Bu email adresi zaten kayıtlı.");

        var user = new BusinessUser(
            request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Password),
            request.Role);

        user.UpdateProfile(request.ContactName, request.BusinessName);

        if (request.BeachId.HasValue)
            user.AssignToBeach(request.BeachId.Value);

        _db.BusinessUsers.Add(user);
        await _db.SaveChangesAsync();

        // Register sonrası token üret (Login ile aynı mantık)
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken(
            user.Id, refreshTokenStr,
            DateTime.UtcNow.AddDays(7), ipAddress, userAgent));

        await _db.SaveChangesAsync();

        return ServiceResult<AuthResponse>.SuccessResult(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenStr,
            Email = user.Email,
            Role = user.Role,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15)
        }, "Kayıt başarılı.");
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(
        string refreshTokenStr, string ipAddress, string userAgent)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var hashedToken = RefreshToken.HashToken(refreshTokenStr);
            var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken);

            if (refreshToken == null)
                return ServiceResult<AuthResponse>.FailureResult("Geçersiz refresh token.");

            // KRITIK: Revoked token → Reuse Attack → Tüm sessionları kapat
            if (refreshToken.IsRevoked)
            {
                _logger.LogCritical("TOKEN REUSE ATTACK! UserId={UserId} IP={IP}", refreshToken.UserId, ipAddress);
                await InvalidateAllSessionsAsync(refreshToken.UserId, "reuse_attack");
                await transaction.CommitAsync();
                return ServiceResult<AuthResponse>.FailureResult(
                    "Güvenlik ihlali: Tüm oturumlar sonlandırıldı. Lütfen tekrar giriş yapın.");
            }

            if (refreshToken.IsExpired)
                return ServiceResult<AuthResponse>.FailureResult("Refresh token süresi dolmuş.");

            var user = await _db.BusinessUsers.FindAsync(refreshToken.UserId);
            if (user == null || !user.IsActive)
                return ServiceResult<AuthResponse>.FailureResult("Kullanıcı bulunamadı veya pasif.");

            // Token Rotation
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshTokenStr = _tokenService.GenerateRefreshToken();

            refreshToken.RevokeAndReplace(newRefreshTokenStr, "rotation"); // eski revoke

            _db.RefreshTokens.Add(new RefreshToken(
                user.Id, newRefreshTokenStr,
                DateTime.UtcNow.AddDays(7), ipAddress, userAgent));

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<AuthResponse>.SuccessResult(new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenStr,
                Email = user.Email,
                Role = user.Role,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15)
            }, "Token yenilendi.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "RefreshToken error");
            return ServiceResult<AuthResponse>.FailureResult("Token yenileme hatası.");
        }
    }

    public async Task LogoutAsync(string? accessToken, string? refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var hashedToken = RefreshToken.HashToken(refreshToken);
            var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken);
            if (token != null)
            {
                token.Revoke("logout");
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task<ServiceResult<bool>> RevokeTokenAsync(string refreshToken, string ipAddress, string reason = "logout")
    {
        var hashedToken = RefreshToken.HashToken(refreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken);

        if (token == null)
            return ServiceResult<bool>.FailureResult("Token bulunamadı.");

        token.Revoke(reason);
        await _db.SaveChangesAsync();

        return ServiceResult<bool>.SuccessResult(true, "Token iptal edildi.");
    }

    private async Task InvalidateAllSessionsAsync(int userId, string reason)
    {
        await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.RevokedAt, DateTime.UtcNow)
                .SetProperty(rt => rt.RevokedReason, reason));
    }
}
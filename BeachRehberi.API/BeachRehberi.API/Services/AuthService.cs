using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.API.Services;

public class AuthService : IAuthService
{
    private readonly BeachDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(BeachDbContext db, ITokenService tokenService, IOtpService otpService, IEmailService emailService, ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _otpService = otpService;
        _emailService = emailService;
        _logger = logger;
    }

    private static string ComputeSha256(string raw)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return string.Concat(bytes.Select(b => b.ToString("x2")));
    }

    public async Task<AuthResult> VerifyEmailByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new AuthResult { Success = false, Message = "Token gereklidir." };

        var tokenHash = ComputeSha256(token);
        var code = await _db.VerificationCodes
            .Where(c => c.CodeHash == tokenHash
                     && c.Purpose == OtpPurpose.EmailVerification
                     && !c.IsUsed)
            .FirstOrDefaultAsync();

        if (code == null || code.ExpiresAt <= DateTime.UtcNow)
            return new AuthResult { Success = false, Message = "Doğrulama bağlantısı geçersiz veya süresi dolmuş." };

        return await VerifyEmailAsync(code.Email, token);
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email))
            return new AuthResult { Success = false, Message = "Bu email adresi zaten kayıtlı." };

        var user = new BusinessUser(
            request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Password),
            UserRoles.User);

        user.UpdatePersonalInfo(request.FirstName, request.LastName, request.PhoneNumber);

        _db.BusinessUsers.Add(user);
        await _db.SaveChangesAsync();

        var token = await _otpService.GenerateTokenAsync(user.Email, "EmailVerification");
        var displayName = $"{request.FirstName} {request.LastName}".Trim();
        await _emailService.SendEmailVerificationAsync(user.Email, displayName, token);

        return new AuthResult { Success = true, Message = "Kayıt başarılı. Lütfen email adresinize gönderilen doğrulama linkine tıklayın." };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _db.BusinessUsers
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Geçersiz kullanıcı bilgileri."); 

        await InvalidateAllSessionsAsync(user.Id, "new_login");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken(
            user.Id, refreshTokenStr,
            DateTime.UtcNow.AddDays(7), "unknown", "unknown"));

        await _db.SaveChangesAsync();

        return new AuthResult
        {
            Success = true,
            Message = "Giriş başarılı.",
            Token = accessToken,
            RefreshToken = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                IsEmailVerified = user.IsEmailVerified
            }
        };
    }

    public async Task<AuthResult> ForgotPasswordAsync(string email)
    {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            var token = await _otpService.GenerateTokenAsync(email, "PasswordReset");
            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            await _emailService.SendPasswordResetAsync(email, displayName, token);
        }

        return new AuthResult { Success = true };
    }

    public async Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var isValid = await _otpService.ValidateTokenAsync(email, "PasswordReset", token);
        if (!isValid)
            return new AuthResult { Success = false, Message = "Invalid or expired token" };

        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(newPassword));
            await _db.SaveChangesAsync();
            await _otpService.InvalidateTokenAsync(email, "PasswordReset");
        }

        return new AuthResult { Success = true };
    }

    public async Task<AuthResult> VerifyEmailAsync(string email, string token)
    {
        var isValid = await _otpService.ValidateTokenAsync(email, "EmailVerification", token);
        if (!isValid)
            return new AuthResult { Success = false, Message = "Invalid or expired token" };

        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            user.VerifyEmail();
            await _db.SaveChangesAsync();
            await _otpService.InvalidateTokenAsync(email, "EmailVerification");
        }

        return new AuthResult { Success = true };
    }

    public async Task<AuthResult> ResendVerificationAsync(string email)
    {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return new AuthResult { Success = true };

        if (user.IsEmailVerified)
            return new AuthResult { Success = false, Message = "Already verified" };

        var token = await _otpService.GenerateTokenAsync(email, "EmailVerification");
        var displayName = $"{user.FirstName} {user.LastName}".Trim();
        await _emailService.SendEmailVerificationAsync(email, displayName, token);

        return new AuthResult { Success = true };
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(
        string refreshTokenStr, string ipAddress, string userAgent)
    {
        var hashedToken = RefreshToken.HashToken(refreshTokenStr);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken);

        if (token == null || !token.IsActive)
            return ServiceResult<AuthResponse>.FailureResult("Geçersiz veya süresi dolmuş refresh token.");

        var user = await _db.BusinessUsers.FindAsync(token.UserId);
        if (user == null || !user.IsActive)
            return ServiceResult<AuthResponse>.FailureResult("Kullanıcı bulunamadı veya pasif durumda.");

        // Token rotation
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenStr = _tokenService.GenerateRefreshToken();

        token.RevokeAndReplace(newRefreshTokenStr, "rotation");
        
        var newRefreshToken = new RefreshToken(
            user.Id, newRefreshTokenStr,
            DateTime.UtcNow.AddDays(7), ipAddress, userAgent);

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync();

        return ServiceResult<AuthResponse>.SuccessResult(new AuthResponse
        {
            Success = true,
            Message = "Token yenilendi.",
            Token = newAccessToken,
            RefreshToken = newRefreshTokenStr
        });
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

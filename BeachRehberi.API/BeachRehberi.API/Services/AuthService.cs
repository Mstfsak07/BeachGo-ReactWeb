using System;
using System.Linq;
using System.Threading.Tasks;
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
    private readonly ILogger<AuthService> _logger;

    public AuthService(BeachDbContext db, ITokenService tokenService, IOtpService otpService, ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _otpService = otpService;
        _logger = logger;
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

        // New token verification logic
        await _otpService.GenerateTokenAsync(user.Email, "EmailVerification");

        return new AuthResult { Success = true, Message = "Kayıt başarılı. Lütfen email adresinize gönderilen kod ile hesabınızı doğrulayın." };
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

    public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user != null)
        {
            var token = await _otpService.GenerateTokenAsync(request.Email, "PasswordReset");
            _logger.LogInformation("Mock Email/SMS sent. Password reset token for {Email} is: {Token}", request.Email, token);
        }
        
        return new AuthResult { Success = true, Message = "Reset link sent" };
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var isValid = await _otpService.ValidateTokenAsync(request.Email, "PasswordReset", request.Token);
        if (!isValid)
            return new AuthResult { Success = false, Message = "Invalid or expired token" };

        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user != null)
        {
            user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
            await _db.SaveChangesAsync();
            await _otpService.InvalidateTokenAsync(request.Email, "PasswordReset");
        }

        return new AuthResult { Success = true, Message = "Password reset successful" };
    }

    public async Task<AuthResult> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var isValid = await _otpService.ValidateTokenAsync(request.Email, "EmailVerification", request.Token);
        if (!isValid)
            return new AuthResult { Success = false };

        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user != null)
        {
            user.VerifyEmail();
            await _db.SaveChangesAsync();
            await _otpService.InvalidateTokenAsync(request.Email, "EmailVerification");
        }

        return new AuthResult { Success = true, Message = "Email verified" };
    }

    public async Task<AuthResult> ResendVerificationAsync(ResendVerificationRequest request)
    {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return new AuthResult { Success = true, Message = "Verification link sent" };

        if (user.IsEmailVerified)
            return new AuthResult { Success = false, Message = "Already verified" };

        var token = await _otpService.GenerateTokenAsync(request.Email, "EmailVerification");
        _logger.LogInformation("Mock Email/SMS sent. Email verification token for {Email} is: {Token}", request.Email, token);

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
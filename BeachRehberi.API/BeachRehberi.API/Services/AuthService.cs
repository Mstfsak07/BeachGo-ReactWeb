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

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email))
            return new AuthResponse { Success = false, Message = "Bu email adresi zaten kayıtlı." };

        var user = new BusinessUser(
            request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Password),
            UserRoles.User);

        user.UpdatePersonalInfo(request.FirstName, request.LastName, request.PhoneNumber);

        _db.BusinessUsers.Add(user);
        await _db.SaveChangesAsync();

        // Send OTP
        await _otpService.GenerateOtpAsync(user.Email, OtpPurpose.EmailVerification);

        return new AuthResponse { Success = true, Message = "Kayıt başarılı. Lütfen email adresinize gönderilen kod ile hesabınızı doğrulayın." };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.BusinessUsers
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Geçersiz kullanıcı bilgileri."); // Controller'da handle edilecek veya BadRequest dönecek

        await InvalidateAllSessionsAsync(user.Id, "new_login");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken(
            user.Id, refreshTokenStr,
            DateTime.UtcNow.AddDays(7), "unknown", "unknown"));

        await _db.SaveChangesAsync();

        return new LoginResponse
        {
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

    public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user != null)
        {
            await _otpService.GenerateOtpAsync(user.Email, OtpPurpose.PasswordReset);
        }
        
        return new AuthResponse { Success = true, Message = "Şifre sıfırlama kodu gönderildi." };
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var isValid = await _otpService.ValidateOtpAsync(request.Email, request.OtpCode, OtpPurpose.PasswordReset);
        if (!isValid)
            return new AuthResponse { Success = false, Message = "Geçersiz veya süresi dolmuş kod." };

        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user != null)
        {
            user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
            await _db.SaveChangesAsync();
        }

        return new AuthResponse { Success = true, Message = "Şifre başarıyla güncellendi." };
    }

    public async Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var isValid = await _otpService.ValidateOtpAsync(request.Email, request.OtpCode, OtpPurpose.EmailVerification);
        if (!isValid)
            return new AuthResponse { Success = false, Message = "Geçersiz veya süresi dolmuş kod." };

        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user != null)
        {
            user.VerifyEmail();
            await _db.SaveChangesAsync();
        }

        return new AuthResponse { Success = true, Message = "Email adresi doğrulandı." };
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(
        string refreshTokenStr, string ipAddress, string userAgent)
    {
        throw new NotImplementedException();
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
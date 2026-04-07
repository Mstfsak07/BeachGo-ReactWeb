using System.Threading.Tasks;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResult> VerifyEmailAsync(VerifyEmailRequest request);
    Task<AuthResult> ResendVerificationAsync(ResendVerificationRequest request);
    
    // Kept for backward compatibility
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);
    Task<ServiceResult<bool>> RevokeTokenAsync(string refreshToken, string ipAddress, string reason = "logout");
    Task LogoutAsync(string? accessToken, string? refreshToken);
}
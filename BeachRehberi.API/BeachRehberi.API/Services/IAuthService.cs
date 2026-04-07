using System.Threading.Tasks;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> ForgotPasswordAsync(string email);
    Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword);
    Task<AuthResult> VerifyEmailAsync(string email, string token);
    Task<AuthResult> ResendVerificationAsync(string email);
    
    // Kept for backward compatibility
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);
    Task<ServiceResult<bool>> RevokeTokenAsync(string refreshToken, string ipAddress, string reason = "logout");
    Task LogoutAsync(string? accessToken, string? refreshToken);
}

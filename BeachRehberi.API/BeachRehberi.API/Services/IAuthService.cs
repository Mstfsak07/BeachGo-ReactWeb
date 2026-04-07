using System.Threading.Tasks;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request);
    
    // Kept for backward compatibility
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);
    Task<ServiceResult<bool>> RevokeTokenAsync(string refreshToken, string ipAddress, string reason = "logout");
    Task LogoutAsync(string? accessToken, string? refreshToken);
}
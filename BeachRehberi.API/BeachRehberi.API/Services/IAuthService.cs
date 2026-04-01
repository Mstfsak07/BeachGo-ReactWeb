using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(string email, string password, string ipAddress, string userAgent);
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);
    Task<ServiceResult<bool>> RevokeTokenAsync(string refreshToken, string ipAddress, string reason = "logout"); // ← YENİ
    Task LogoutAsync(string? accessToken, string? refreshToken);
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, string ipAddress, string userAgent);
}
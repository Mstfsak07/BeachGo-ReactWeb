using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(string email, string password, string ipAddress, string userAgent);
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);
    Task LogoutAsync(string? accessToken, string? refreshToken);
    Task<ServiceResult<BusinessUser>> RegisterAsync(RegisterRequest request);
}

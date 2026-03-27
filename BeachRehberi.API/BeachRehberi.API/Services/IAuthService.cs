using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string? accessToken, string? refreshToken);
    Task<ApiResponse<BusinessUser>> RegisterAsync(RegisterRequest request);
}

using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;
public interface IAuthService {
    Task<AuthResponse?> LoginAsync(string email, string password);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string? accessToken, string? refreshToken);
    Task<BusinessUser?> RegisterAsync(RegisterRequest request);   
}

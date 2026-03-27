using BeachRehberi.API.Models;
namespace BeachRehberi.API.Services;
public interface IAuthService {
    Task<AuthResponse?> LoginAsync(string email, string password);
    Task<BusinessUser?> RegisterAsync(RegisterRequest request);
}
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IAuthService
{
    Task<string?> LoginAsync(string email, string password);
    Task<BusinessUser?> GetUserAsync(string email);
}
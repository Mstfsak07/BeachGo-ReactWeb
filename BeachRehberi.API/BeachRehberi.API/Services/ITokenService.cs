using BeachRehberi.API.Models;
using System.Security.Claims;

namespace BeachRehberi.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(BusinessUser user);
    string GenerateRefreshToken();
    Task BlacklistTokenAsync(string token, DateTime expiry);
    Task<bool> IsTokenBlacklistedAsync(string token);
}

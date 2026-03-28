using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(BusinessUser user);
    string GenerateRefreshToken();
    ClaimsPrincipalResult? ValidateExpiredAccessToken(string accessToken); // ← YENİ
    Task BlacklistTokenAsync(string token, DateTime expiry);
    Task<bool> IsTokenBlacklistedAsync(string token);
}

public class ClaimsPrincipalResult
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Jti { get; init; } = string.Empty;
}
using System.Security.Claims;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(BusinessUser user);
    string GenerateRefreshToken();
    ClaimsPrincipalResult? ValidateExpiredAccessToken(string accessToken);
    Task BlacklistTokenAsync(string token, DateTime expiry);
    Task<bool> IsTokenBlacklistedAsync(string token);
    Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
    Task RevokeRefreshTokenAsync(int userId, string refreshToken);
    Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    // GOREV Yeni Metodlar
    Task RevokeAccessToken(string jti);
    Task<bool> IsTokenRevoked(string jti);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshToken(string token);
    Task RevokeAccessTokenAsync(string token);
    Task<bool> IsTokenRevokedAsync(string token);
    Task RevokeRefreshTokenAsync(string refreshToken);
}

public class ClaimsPrincipalResult
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Jti { get; init; } = string.Empty;
}

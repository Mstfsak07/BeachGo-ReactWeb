using System.Text;
using BeachRehberi.API.Middlewares;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.Http;

namespace BeachRehberi.API.Tests;

public class JwtBlacklistMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_returns_401_when_token_is_revoked()
    {
        var nextCalled = false;
        var middleware = new JwtBlacklistMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer revoked-token";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, new FakeTokenService(isRevoked: true));

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(nextCalled);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync();
        Assert.Contains("Oturumunuz", payload, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeTokenService : ITokenService
    {
        private readonly bool _isRevoked;

        public FakeTokenService(bool isRevoked)
        {
            _isRevoked = isRevoked;
        }

        public string GenerateAccessToken(BeachRehberi.API.Models.BusinessUser user) => string.Empty;
        public string GenerateRefreshToken() => string.Empty;
        public ClaimsPrincipalResult? ValidateExpiredAccessToken(string accessToken) => null;
        public Task BlacklistTokenAsync(string token, DateTime expiry) => Task.CompletedTask;
        public Task<bool> IsTokenBlacklistedAsync(string token) => Task.FromResult(_isRevoked);
        public Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken) => Task.FromResult(false);
        public Task RevokeRefreshTokenAsync(int userId, string refreshToken) => Task.CompletedTask;
        public Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry) => Task.CompletedTask;
        public System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token) => null;
        public Task RevokeAccessToken(string jti) => Task.CompletedTask;
        public Task<bool> IsTokenRevoked(string jti) => Task.FromResult(_isRevoked);
        public Task<BeachRehberi.API.Models.AuthResult> RefreshTokenAsync(string refreshToken) => Task.FromResult(BeachRehberi.API.Models.AuthResult.Failure("n/a"));
        public Task RevokeRefreshToken(string token) => Task.CompletedTask;
        public Task RevokeAccessTokenAsync(string token) => Task.CompletedTask;
        public Task<bool> IsTokenRevokedAsync(string token) => Task.FromResult(_isRevoked);
        public Task RevokeRefreshTokenAsync(string refreshToken) => Task.CompletedTask;
    }
}

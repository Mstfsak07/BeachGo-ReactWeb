using BeachRehberi.API.Data;
using BeachRehberi.API.Features.Auth.Commands.Login;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Tests;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_blocks_unverified_business_user_login()
    {
        await using var db = CreateDbContext();
        db.BusinessUsers.Add(new BusinessUser(
            "business@example.com",
            BCrypt.Net.BCrypt.HashPassword("Password123!"),
            UserRoles.Business));
        await db.SaveChangesAsync();

        var handler = new LoginHandler(db, new FakeTokenService());

        var result = await handler.Handle(
            new LoginCommand(new LoginRequest
            {
                Email = "business@example.com",
                Password = "Password123!"
            }),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Giriş yapmadan önce e-posta adresinizi doğrulamanız gerekiyor.", result.Message);
    }

    [Fact]
    public async Task Handle_blocks_unverified_regular_user_login()
    {
        await using var db = CreateDbContext();
        db.BusinessUsers.Add(new BusinessUser(
            "user@example.com",
            BCrypt.Net.BCrypt.HashPassword("Password123!"),
            UserRoles.User));
        await db.SaveChangesAsync();

        var handler = new LoginHandler(db, new FakeTokenService());

        var result = await handler.Handle(
            new LoginCommand(new LoginRequest
            {
                Email = "user@example.com",
                Password = "Password123!"
            }),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Giriş yapmadan önce e-posta adresinizi doğrulamanız gerekiyor.", result.Message);
    }

    [Fact]
    public async Task Handle_matches_business_email_case_insensitively()
    {
        await using var db = CreateDbContext();
        var user = new BusinessUser(
            "Business@Example.com",
            BCrypt.Net.BCrypt.HashPassword("Password123!"),
            UserRoles.Business);
        user.VerifyEmail();
        db.BusinessUsers.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginHandler(db, new FakeTokenService());

        var result = await handler.Handle(
            new LoginCommand(new LoginRequest
            {
                Email = " business@example.com ",
                Password = "Password123!"
            }),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(UserRoles.Business, result.User?.Role);
        Assert.Equal(UserRoles.Business, result.User?.AccountType);
    }

    private static BeachDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BeachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeachDbContext(options);
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenerateAccessToken(BusinessUser user) => "access-token";
        public string GenerateRefreshToken() => "refresh-token";
        public ClaimsPrincipalResult? ValidateExpiredAccessToken(string accessToken) => null;
        public Task BlacklistTokenAsync(string token, DateTime expiry) => Task.CompletedTask;
        public Task<bool> IsTokenBlacklistedAsync(string token) => Task.FromResult(false);
        public Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken) => Task.FromResult(true);
        public Task RevokeRefreshTokenAsync(int userId, string refreshToken) => Task.CompletedTask;
        public Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry) => Task.CompletedTask;
        public System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token) => null;
        public Task RevokeAccessToken(string jti) => Task.CompletedTask;
        public Task<bool> IsTokenRevoked(string jti) => Task.FromResult(false);
        public Task<AuthResult> RefreshTokenAsync(string refreshToken) => Task.FromResult(new AuthResult());
        public Task RevokeRefreshToken(string token) => Task.CompletedTask;
        public Task RevokeAccessTokenAsync(string token) => Task.CompletedTask;
        public Task<bool> IsTokenRevokedAsync(string token) => Task.FromResult(false);
        public Task RevokeRefreshTokenAsync(string refreshToken) => Task.CompletedTask;
    }
}

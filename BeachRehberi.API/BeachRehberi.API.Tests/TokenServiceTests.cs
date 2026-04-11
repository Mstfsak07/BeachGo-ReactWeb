using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BeachRehberi.API.Tests;

public class TokenServiceTests
{
    [Fact]
    public async Task RevokeAccessTokenAsync_blacklists_token_by_jti()
    {
        await using var db = CreateDbContext();
        var service = new TokenService(
            db,
            new MemoryCache(new MemoryCacheOptions()),
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "this-is-a-test-secret-key-with-at-least-32-characters",
                    ["Jwt:AccessTokenExpiryMinutes"] = "15",
                    ["Jwt:RefreshTokenExpiryDays"] = "7"
                })
                .Build(),
            NullLogger<TokenService>.Instance);

        var token = service.GenerateAccessToken(new BusinessUser("admin@example.com", "hash", UserRoles.Admin));

        await service.RevokeAccessTokenAsync(token);

        Assert.True(await service.IsTokenRevokedAsync(token));
        Assert.Single(db.RevokedTokens);
    }

    private static BeachDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BeachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeachDbContext(options);
    }
}

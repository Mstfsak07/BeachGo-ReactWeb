using BeachRehberi.API.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public sealed class RevokedTokenCleanupJob : IRevokedTokenCleanupJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RevokedTokenCleanupJob> _logger;

    public RevokedTokenCleanupJob(IServiceScopeFactory scopeFactory, ILogger<RevokedTokenCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<int> CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BeachDbContext>();
        var cutoff = DateTime.UtcNow;
        var expiredTokens = await db.RevokedTokens
            .Where(x => x.ExpiresAt <= cutoff)
            .ToListAsync(cancellationToken);

        if (expiredTokens.Count == 0)
        {
            _logger.LogInformation("Revoked token cleanup found no expired entries.");
            return 0;
        }

        db.RevokedTokens.RemoveRange(expiredTokens);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Revoked token cleanup removed {Count} expired entries.", expiredTokens.Count);
        return expiredTokens.Count;
    }
}

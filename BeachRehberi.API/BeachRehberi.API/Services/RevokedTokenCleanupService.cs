using BeachRehberi.API.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public sealed class RevokedTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RevokedTokenCleanupService> _logger;

    public RevokedTokenCleanupService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<RevokedTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BeachDbContext>();
            var cutoff = DateTime.UtcNow;
            var expiredTokens = await db.RevokedTokens
                .Where(x => x.ExpiresAt <= cutoff)
                .ToListAsync(cancellationToken);

            if (expiredTokens.Count == 0)
                return;

            db.RevokedTokens.RemoveRange(expiredTokens);
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Revoked token cleanup removed {Count} expired entries.", expiredTokens.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Revoked token cleanup failed.");
        }
    }
}

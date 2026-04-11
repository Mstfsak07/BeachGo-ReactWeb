using BeachRehberi.API.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public sealed class RevokedTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RevokedTokenCleanupService> _logger;
    private readonly int _accessTokenExpiryMinutes;

    public RevokedTokenCleanupService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<RevokedTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var jwtSettings = configuration.GetSection("JwtSettings");
        _accessTokenExpiryMinutes = jwtSettings.GetValue<int?>("AccessTokenExpiryMinutes")
            ?? configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
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
            var cutoff = DateTime.UtcNow.AddMinutes(-_accessTokenExpiryMinutes);
            var expiredTokens = await db.RevokedTokens
                .Where(x => x.RevokedAt <= cutoff)
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

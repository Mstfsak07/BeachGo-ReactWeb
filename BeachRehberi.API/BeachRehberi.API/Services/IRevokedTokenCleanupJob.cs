namespace BeachRehberi.API.Services;

public interface IRevokedTokenCleanupJob
{
    Task<int> CleanupAsync(CancellationToken cancellationToken);
}

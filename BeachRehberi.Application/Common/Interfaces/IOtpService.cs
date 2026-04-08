namespace BeachRehberi.Application.Common.Interfaces;

public interface IOtpService
{
    Task<string> GenerateTokenAsync(string email, string purpose, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string email, string purpose, string token, CancellationToken cancellationToken = default);
    Task InvalidateTokenAsync(string email, string purpose, CancellationToken cancellationToken = default);
}

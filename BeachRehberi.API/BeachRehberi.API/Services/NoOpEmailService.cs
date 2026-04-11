using Microsoft.Extensions.Logging;

namespace BeachRehberi.API.Services;

/// <summary>
/// No-op email service for development/testing when Resend API key is not configured.
/// Logs emails to console instead of sending them.
/// </summary>
public class NoOpEmailService :
    BeachRehberi.Application.Common.Interfaces.IEmailService,
    BeachRehberi.API.Services.IEmailService
{
    private readonly ILogger<NoOpEmailService> _logger;
    private readonly IConfiguration _configuration;

    public NoOpEmailService(ILogger<NoOpEmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    // API.Services.IEmailService methods
    public Task SendEmailVerificationAsync(string toEmail, string toName, string token)
    {
        var verifyLink = $"{GetAppUrl()}/verify-email?token={Uri.EscapeDataString(token)}";
        _logger.LogInformation("[NoOp Email] Verification email to {Email} ({Name}): link={Link}", toEmail, toName, verifyLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string toName, string token)
    {
        var resetLink = $"{GetAppUrl()}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(toEmail)}";
        _logger.LogInformation("[NoOp Email] Password reset email to {Email} ({Name}): link={Link}", toEmail, toName, resetLink);
        return Task.CompletedTask;
    }

    // Application.Common.Interfaces.IEmailService methods
    public Task SendWelcomeEmailAsync(string toEmail, string fullName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp Email] Welcome email to {Email} ({Name})", toEmail, fullName);
        return Task.CompletedTask;
    }

    public Task SendReservationConfirmationAsync(string toEmail, string fullName, string beachName, DateTime reservationDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp Email] Reservation confirmation to {Email}: {Beach} on {Date}", toEmail, beachName, reservationDate);
        return Task.CompletedTask;
    }

    public Task SendReservationStatusUpdateAsync(string toEmail, string fullName, string beachName, string status, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp Email] Reservation status update to {Email}: {Beach} - {Status}", toEmail, beachName, status);
        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(string toEmail, string fullName, string token, CancellationToken cancellationToken = default)
    {
        var verifyLink = $"{GetAppUrl()}/verify-email?token={Uri.EscapeDataString(token)}";
        _logger.LogInformation("[NoOp Email] Verification email to {Email} ({Name}): link={Link}", toEmail, fullName, verifyLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string fullName, string token, CancellationToken cancellationToken = default)
    {
        var resetLink = $"{GetAppUrl()}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(toEmail)}";
        _logger.LogInformation("[NoOp Email] Password reset email to {Email} ({Name}): link={Link}", toEmail, fullName, resetLink);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp Email] Generic email to {Email}: subject={Subject}", toEmail, subject);
        return Task.CompletedTask;
    }

    private string GetAppUrl()
    {
        return Environment.GetEnvironmentVariable("APP_URL")
               ?? _configuration["App:Url"]
               ?? "http://localhost:3000";
    }
}

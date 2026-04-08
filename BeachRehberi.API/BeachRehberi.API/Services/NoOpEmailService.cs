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

    public NoOpEmailService(ILogger<NoOpEmailService> logger)
    {
        _logger = logger;
    }

    // API.Services.IEmailService methods
    public Task SendEmailVerificationAsync(string toEmail, string toName, string token)
    {
        _logger.LogInformation("[NoOp Email] Verification email to {Email} ({Name}): token={Token}", toEmail, toName, token);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string toName, string token)
    {
        _logger.LogInformation("[NoOp Email] Password reset email to {Email} ({Name}): token={Token}", toEmail, toName, token);
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
        _logger.LogInformation("[NoOp Email] Verification email to {Email} ({Name}): token={Token}", toEmail, fullName, token);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string fullName, string token, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp Email] Password reset email to {Email} ({Name}): token={Token}", toEmail, fullName, token);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp Email] Generic email to {Email}: subject={Subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}

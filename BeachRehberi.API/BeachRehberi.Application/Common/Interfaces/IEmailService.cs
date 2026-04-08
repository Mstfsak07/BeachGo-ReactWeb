namespace BeachRehberi.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(
        string toEmail,
        string fullName,
        CancellationToken cancellationToken = default);

    Task SendReservationConfirmationAsync(
        string toEmail,
        string fullName,
        string beachName,
        DateTime reservationDate,
        CancellationToken cancellationToken = default);

    Task SendReservationStatusUpdateAsync(
        string toEmail,
        string fullName,
        string beachName,
        string status,
        CancellationToken cancellationToken = default);

    Task SendEmailVerificationAsync(
        string toEmail,
        string fullName,
        string token,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        string toEmail,
        string fullName,
        string token,
        CancellationToken cancellationToken = default);

    Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}

namespace BeachRehberi.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendReservationConfirmationAsync(string toEmail, string userName, string beachName, DateTime reservationDate, CancellationToken cancellationToken = default);
    Task SendReservationStatusUpdateAsync(string toEmail, string userName, string beachName, string status, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string toEmail, string userName, string resetToken, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default);
}

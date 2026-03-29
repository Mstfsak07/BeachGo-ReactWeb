using BeachRehberi.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendReservationConfirmationAsync(string toEmail, string userName,
        string beachName, DateTime reservationDate, CancellationToken cancellationToken = default)
    {
        // TODO: SMTP veya SendGrid entegrasyonu
        _logger.LogInformation(
            "📧 Rezervasyon onay e-postası gönderildi → {Email} | Plaj: {Beach} | Tarih: {Date}",
            toEmail, beachName, reservationDate.ToString("dd.MM.yyyy"));
        await Task.CompletedTask;
    }

    public async Task SendReservationStatusUpdateAsync(string toEmail, string userName,
        string beachName, string status, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "📧 Rezervasyon durum güncellemesi → {Email} | Plaj: {Beach} | Durum: {Status}",
            toEmail, beachName, status);
        await Task.CompletedTask;
    }

    public async Task SendPasswordResetAsync(string toEmail, string userName,
        string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "📧 Şifre sıfırlama e-postası gönderildi → {Email}", toEmail);
        await Task.CompletedTask;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "📧 Hoş geldin e-postası gönderildi → {Email} | Kullanıcı: {Name}", toEmail, userName);
        await Task.CompletedTask;
    }
}

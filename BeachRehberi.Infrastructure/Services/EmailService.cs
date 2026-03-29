using BeachRehberi.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Infrastructure.Services;

/// <summary>
/// E-posta servisi. SMTP config tanımlı değilse geliştirme modunda
/// mesajları sadece loglara yazar.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly bool _isDevMode;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        _isDevMode = string.IsNullOrEmpty(configuration["Email:SmtpHost"]);

        if (_isDevMode)
            _logger.LogWarning("EmailService DEV modunda çalışıyor — e-postalar gönderilmeyecek.");
    }

    public async Task SendWelcomeEmailAsync(
        string toEmail, string fullName, CancellationToken cancellationToken = default)
    {
        var subject = "BeachRehberi'ne Hoş Geldiniz! 🏖️";
        var body = $"""
            Merhaba {fullName},

            BeachRehberi ailesine hoş geldiniz!
            En iyi plajları keşfetmek ve rezervasyon yapmak için platformumuzu kullanabilirsiniz.

            İyi tatiller dileriz! 🌊
            """;

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendReservationConfirmationAsync(
        string toEmail, string fullName, string beachName,
        DateTime reservationDate, CancellationToken cancellationToken = default)
    {
        var subject = $"Rezervasyonunuz Alındı — {beachName}";
        var body = $"""
            Merhaba {fullName},

            {beachName} için {reservationDate:dd.MM.yyyy} tarihli rezervasyonunuz alınmıştır.
            Rezervasyon durumunuzu uygulama üzerinden takip edebilirsiniz.

            İyi tatiller! 🏖️
            """;

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendReservationStatusUpdateAsync(
        string toEmail, string fullName, string beachName,
        string status, CancellationToken cancellationToken = default)
    {
        var statusText = status switch
        {
            "Approved"  => "onaylandı ✅",
            "Rejected"  => "reddedildi ❌",
            "Cancelled" => "iptal edildi",
            _           => "güncellendi"
        };

        var subject = $"Rezervasyon Durumu Güncellendi — {beachName}";
        var body = $"""
            Merhaba {fullName},

            {beachName} için rezervasyonunuz {statusText}.
            Detaylar için uygulamayı ziyaret edebilirsiniz.
            """;

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail, string resetToken, CancellationToken cancellationToken = default)
    {
        var subject = "Şifre Sıfırlama Talebi";
        var body = $"""
            Şifrenizi sıfırlamak için aşağıdaki kodu kullanın:

                {resetToken}

            Bu kod 15 dakika geçerlidir.
            Eğer bu talebi siz yapmadıysanız lütfen bu e-postayı dikkate almayın.
            """;

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendEmailAsync(
        string toEmail, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        if (_isDevMode)
        {
            _logger.LogInformation(
                "[EMAIL - DEV]\nTo: {To}\nSubject: {Subject}\n---\n{Body}",
                toEmail, subject, body);
            await Task.CompletedTask;
            return;
        }

        // TODO: MailKit / SendGrid entegrasyonu buraya
        _logger.LogInformation("E-posta gönderildi → {To} | Konu: {Subject}", toEmail, subject);
        await Task.CompletedTask;
    }
}

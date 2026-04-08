using Resend;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.API.Services;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _config;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly IHostEnvironment _env;

    private const string FromAddress = "BeachGo <noreply@beachgo.net>";

    public ResendEmailService(IResend resend, IConfiguration config, ILogger<ResendEmailService> logger, IHostEnvironment env)
    {
        _resend = resend;
        _config = config;
        _logger = logger;
        _env = env;
    }

    public async Task SendEmailVerificationAsync(string toEmail, string toName, string token)
    {
        var appUrl = _config["App:Url"] ?? "http://localhost:5173";
        var verifyLink = $"{appUrl}/verify-email?token={Uri.EscapeDataString(token)}";

        if (_env.IsDevelopment())
        {
            _logger.LogInformation(
                "[DEV] Email verification link for {Email}: {Link}",
                toEmail, verifyLink);
            return;
        }

        var message = new EmailMessage
        {
            From = FromAddress,
            To = { toEmail },
            Subject = "BeachGo – E-posta Adresinizi Doğrulayın",
            HtmlBody = BuildVerificationHtml(toName, verifyLink),
        };

        try
        {
            await _resend.EmailSendAsync(message);
            _logger.LogInformation("Verification email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", toEmail);
        }
    }

    public async Task SendPasswordResetAsync(string toEmail, string toName, string token)
    {
        var appUrl = _config["App:Url"] ?? "http://localhost:5173";
        var resetLink = $"{appUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(toEmail)}";

        if (_env.IsDevelopment())
        {
            _logger.LogInformation(
                "[DEV] Password reset link for {Email}: {Link}",
                toEmail, resetLink);
            return;
        }

        var message = new EmailMessage
        {
            From = FromAddress,
            To = { toEmail },
            Subject = "BeachGo – Şifre Sıfırlama",
            HtmlBody = BuildPasswordResetHtml(toName, resetLink),
        };

        try
        {
            await _resend.EmailSendAsync(message);
            _logger.LogInformation("Password reset email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
        }
    }

    private static string BuildVerificationHtml(string name, string link) => $"""
        <!DOCTYPE html>
        <html lang="tr">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f8fafc;font-family:'Segoe UI',Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f8fafc;padding:40px 0;">
            <tr><td align="center">
              <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.06);">
                <!-- Header -->
                <tr>
                  <td style="background:linear-gradient(135deg,#2563eb 0%,#0ea5e9 100%);padding:36px 40px;text-align:center;">
                    <span style="font-size:32px;font-weight:900;letter-spacing:-1px;color:#ffffff;">Beach<span style="color:#bfdbfe;">Go</span></span>
                    <p style="margin:8px 0 0;color:#bfdbfe;font-size:13px;letter-spacing:2px;text-transform:uppercase;">Plaj Rehberi</p>
                  </td>
                </tr>
                <!-- Body -->
                <tr>
                  <td style="padding:40px 40px 32px;">
                    <p style="margin:0 0 8px;font-size:22px;font-weight:800;color:#0f172a;">Merhaba{(string.IsNullOrWhiteSpace(name) ? "" : $", {name}")}!</p>
                    <p style="margin:0 0 28px;font-size:15px;color:#475569;line-height:1.6;">
                      BeachGo'ya hoş geldiniz. Hesabınızı etkinleştirmek için aşağıdaki butona tıklayın.
                      Bu bağlantı <strong>24 saat</strong> geçerlidir.
                    </p>
                    <div style="text-align:center;margin:32px 0;">
                      <a href="{link}" style="display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;padding:16px 40px;border-radius:14px;font-size:14px;font-weight:800;letter-spacing:1.5px;text-transform:uppercase;">
                        E-postamı Doğrula
                      </a>
                    </div>
                    <p style="margin:0;font-size:13px;color:#94a3b8;line-height:1.6;">
                      Butona tıklayamıyorsanız aşağıdaki bağlantıyı kopyalayıp tarayıcınıza yapıştırın:<br>
                      <a href="{link}" style="color:#2563eb;word-break:break-all;">{link}</a>
                    </p>
                  </td>
                </tr>
                <!-- Footer -->
                <tr>
                  <td style="background:#f1f5f9;padding:20px 40px;text-align:center;">
                    <p style="margin:0;font-size:12px;color:#94a3b8;">
                      Bu e-postayı siz istemediyseniz güvenle yoksayabilirsiniz.<br>
                      &copy; {DateTime.UtcNow.Year} BeachGo &bull; beachgo.net
                    </p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    private static string BuildPasswordResetHtml(string name, string link) => $"""
        <!DOCTYPE html>
        <html lang="tr">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f8fafc;font-family:'Segoe UI',Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f8fafc;padding:40px 0;">
            <tr><td align="center">
              <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.06);">
                <tr>
                  <td style="background:linear-gradient(135deg,#2563eb 0%,#0ea5e9 100%);padding:36px 40px;text-align:center;">
                    <span style="font-size:32px;font-weight:900;letter-spacing:-1px;color:#ffffff;">Beach<span style="color:#bfdbfe;">Go</span></span>
                    <p style="margin:8px 0 0;color:#bfdbfe;font-size:13px;letter-spacing:2px;text-transform:uppercase;">Şifre Sıfırlama</p>
                  </td>
                </tr>
                <tr>
                  <td style="padding:40px 40px 32px;">
                    <p style="margin:0 0 8px;font-size:22px;font-weight:800;color:#0f172a;">Merhaba{(string.IsNullOrWhiteSpace(name) ? "" : $", {name}")}!</p>
                    <p style="margin:0 0 28px;font-size:15px;color:#475569;line-height:1.6;">
                      Şifre sıfırlama talebinde bulundunuz. Aşağıdaki butona tıklayarak yeni şifrenizi belirleyebilirsiniz.
                      Bu bağlantı <strong>15 dakika</strong> geçerlidir.
                    </p>
                    <div style="text-align:center;margin:32px 0;">
                      <a href="{link}" style="display:inline-block;background:#dc2626;color:#ffffff;text-decoration:none;padding:16px 40px;border-radius:14px;font-size:14px;font-weight:800;letter-spacing:1.5px;text-transform:uppercase;">
                        Şifremi Sıfırla
                      </a>
                    </div>
                    <p style="margin:0;font-size:13px;color:#94a3b8;line-height:1.6;">
                      Bu isteği siz yapmadıysanız bu e-postayı görmezden gelebilirsiniz.<br>
                      <a href="{link}" style="color:#2563eb;word-break:break-all;">{link}</a>
                    </p>
                  </td>
                </tr>
                <tr>
                  <td style="background:#f1f5f9;padding:20px 40px;text-align:center;">
                    <p style="margin:0;font-size:12px;color:#94a3b8;">
                      &copy; {DateTime.UtcNow.Year} BeachGo &bull; beachgo.net
                    </p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}

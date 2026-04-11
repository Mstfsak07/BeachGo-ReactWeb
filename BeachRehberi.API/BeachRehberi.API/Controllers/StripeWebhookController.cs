using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BeachRehberi.API.Controllers;

/// <summary>
/// Stripe webhook: yalnızca ham gövde + Stripe-Signature ile imza doğrulaması yapılır.
/// İş mantığı (payment_intent.succeeded vb.) gerçek ödeme entegrasyonu tamamlandıkça eklenecek.
/// </summary>
[ApiController]
[Route("api/stripe")]
[AllowAnonymous]
public class StripeWebhookController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(IConfiguration config, ILogger<StripeWebhookController> logger)
    {
        _config = config;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        var webhookSecret = _config["Stripe:WebhookSecret"]?.Trim();
        if (string.IsNullOrEmpty(webhookSecret) ||
            webhookSecret.Contains("YOUR_STRIPE", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Stripe webhook çağrıldı ancak Stripe:WebhookSecret yapılandırılmamış.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { success = false, message = "Stripe webhook yapılandırılmamış." });
        }

        Request.EnableBuffering();
        string json;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false,
                   bufferSize: 1024, leaveOpen: true))
        {
            json = await reader.ReadToEndAsync(cancellationToken);
        }

        Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogWarning("Stripe webhook boş gövde ile çağrıldı.");
            return BadRequest(new { success = false, message = "Boş gövde." });
        }

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrEmpty(signatureHeader))
        {
            _logger.LogWarning("Stripe webhook Stripe-Signature başlığı eksik.");
            return BadRequest(new { success = false, message = "Stripe-Signature gerekli." });
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                webhookSecret,
                throwOnApiVersionMismatch: false);

            _logger.LogInformation(
                "Stripe webhook doğrulandı. Type={EventType}, Id={EventId}",
                stripeEvent.Type,
                stripeEvent.Id);

            // Ödeme durumu güncellemeleri burada işlenecek (entegrasyon tamamlanınca).
            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe webhook imza doğrulaması başarısız.");
            return BadRequest(new { success = false, message = "İmza doğrulaması başarısız." });
        }
    }
}

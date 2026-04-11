using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

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
    private readonly BeachDbContext _db;

    public StripeWebhookController(IConfiguration config, ILogger<StripeWebhookController> logger, BeachDbContext db)
    {
        _config = config;
        _logger = logger;
        _db = db;
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

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                await HandleCheckoutSessionCompletedAsync(session, cancellationToken);
            }
            else if (stripeEvent.Type == EventTypes.CheckoutSessionAsyncPaymentFailed)
            {
                var session = stripeEvent.Data.Object as Session;
                await HandleCheckoutSessionFailedAsync(session, cancellationToken);
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe webhook imza doğrulaması başarısız.");
            return BadRequest(new { success = false, message = "İmza doğrulaması başarısız." });
        }
    }

    private async Task HandleCheckoutSessionCompletedAsync(Session? session, CancellationToken cancellationToken)
    {
        if (session?.Metadata == null || !session.Metadata.TryGetValue("reservationId", out var reservationIdText) || !int.TryParse(reservationIdText, out var reservationId))
            return;

        var reservation = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
        if (reservation == null)
            return;

        reservation.PaymentStatus = PaymentStatus.Paid;

        var payment = await _db.ReservationPayments.FirstOrDefaultAsync(x => x.ReservationId == reservationId, cancellationToken);
        if (payment == null)
        {
            payment = new Models.ReservationPayment
            {
                ReservationId = reservationId,
                Amount = reservation.TotalPrice,
                Status = PaymentStatus.Paid,
                TransactionId = session.Id,
                PaymentMethod = "Stripe",
                PaidAt = DateTime.UtcNow
            };
            _db.ReservationPayments.Add(payment);
        }
        else
        {
            payment.Status = PaymentStatus.Paid;
            payment.TransactionId = session.Id;
            payment.PaidAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleCheckoutSessionFailedAsync(Session? session, CancellationToken cancellationToken)
    {
        if (session?.Metadata == null || !session.Metadata.TryGetValue("reservationId", out var reservationIdText) || !int.TryParse(reservationIdText, out var reservationId))
            return;

        var reservation = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
        if (reservation == null)
            return;

        reservation.PaymentStatus = PaymentStatus.Failed;

        var payment = await _db.ReservationPayments.FirstOrDefaultAsync(x => x.ReservationId == reservationId, cancellationToken);
        if (payment != null)
        {
            payment.Status = PaymentStatus.Failed;
            payment.TransactionId = session.Id;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}

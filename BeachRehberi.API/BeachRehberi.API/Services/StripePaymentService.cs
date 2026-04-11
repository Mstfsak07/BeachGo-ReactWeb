using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using BeachRehberi.API.Models.Enums;

namespace BeachRehberi.API.Services;

public class StripePaymentService : IPaymentService
{
    private readonly ILogger<StripePaymentService> _logger;
    private readonly IConfiguration _config;

    public StripePaymentService(ILogger<StripePaymentService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<PaymentProcessResult> ProcessPaymentAsync(int reservationId, decimal amount, string paymentMethod = "Stripe", string? confirmationCode = null)
    {
        var useReal = _config.GetValue<bool>("Features:UseRealPayment");
        var secretKey = _config["Stripe:SecretKey"]?.Trim();
        var appUrl = (Environment.GetEnvironmentVariable("APP_URL") ?? _config["App:Url"] ?? "http://localhost:3000").TrimEnd('/');

        if (!useReal)
        {
            _logger.LogWarning("[PAYMENT] Payment system is currently DISABLED. Reservation: {ResId}", reservationId);
            return PaymentProcessResult.Failed("Ödeme sistemi devre dışı.");
        }

        if (string.IsNullOrWhiteSpace(secretKey) ||
            secretKey.Contains("YOUR_STRIPE", StringComparison.OrdinalIgnoreCase))
        {
            return PaymentProcessResult.Failed("Stripe:SecretKey yapılandırılmamış.");
        }

        StripeConfiguration.ApiKey = secretKey;

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = $"{appUrl}/reservation-success?payment=success&confirmationCode={Uri.EscapeDataString(confirmationCode ?? string.Empty)}&session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{appUrl}/reservation-success?payment=cancel&confirmationCode={Uri.EscapeDataString(confirmationCode ?? string.Empty)}",
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "try",
                        UnitAmount = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"BeachGo Reservation #{reservationId}"
                        }
                    }
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["reservationId"] = reservationId.ToString(),
                ["confirmationCode"] = confirmationCode ?? string.Empty
            }
        });

        _logger.LogInformation("[PAYMENT] Stripe checkout session created. Reservation: {ResId}, Session: {SessionId}", reservationId, session.Id);

        return new PaymentProcessResult
        {
            Success = true,
            RequiresRedirect = true,
            RedirectUrl = session.Url,
            TransactionId = session.Id,
            Status = PaymentStatus.Pending,
            Message = "Stripe ödeme oturumu oluşturuldu."
        };
    }
}

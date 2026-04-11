using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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

    public Task<bool> ProcessPaymentAsync(int reservationId, decimal amount, string paymentMethod = "Stripe")
    {
        var useReal = _config.GetValue<bool>("Features:UseRealPayment");

        if (!useReal)
        {
            _logger.LogWarning("[PAYMENT] Payment system is currently DISABLED. Reservation: {ResId}", reservationId);
            return Task.FromResult(false);
        }

        // TODO: Implement Stripe integration
        _logger.LogError("[PAYMENT] Stripe integration is not complete yet. Cannot process payment for Reservation: {ResId}", reservationId);
        return Task.FromResult(false);
    }
}

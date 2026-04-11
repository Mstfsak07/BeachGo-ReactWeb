using System.Threading.Tasks;

namespace BeachRehberi.API.Services;

public interface IPaymentService
{
    Task<PaymentProcessResult> ProcessPaymentAsync(int reservationId, decimal amount, string paymentMethod = "Stripe", string? confirmationCode = null);
}

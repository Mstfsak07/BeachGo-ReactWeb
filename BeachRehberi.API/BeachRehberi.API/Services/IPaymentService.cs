using System.Threading.Tasks;

namespace BeachRehberi.API.Services;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(int reservationId, decimal amount, string paymentMethod = "Mock");
}

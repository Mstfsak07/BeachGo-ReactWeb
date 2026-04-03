using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.API.Services;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(decimal amount, string currency, string cardToken);
}

public class MockPaymentService : IPaymentService
{
    private readonly ILogger<MockPaymentService> _logger;

    public MockPaymentService(ILogger<MockPaymentService> logger)
    {
        _logger = logger;
    }

    public Task<bool> ProcessPaymentAsync(decimal amount, string currency, string cardToken)
    {
        _logger.LogInformation("[MOCK PAYMENT] Amount: {Amount} {Currency} | Token: {Token}", amount, currency, cardToken);
        return Task.FromResult(true);
    }
}
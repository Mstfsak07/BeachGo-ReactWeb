namespace BeachRehberi.Application.Common.Interfaces;

public interface IStripeService
{
    Task<string> CreateCustomerAsync(string email, string name, CancellationToken cancellationToken = default);
    Task<string> CreateSubscriptionAsync(string customerId, string priceId, CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, CancellationToken cancellationToken = default);
    Task<bool> VerifyWebhookSignatureAsync(string payload, string signature, CancellationToken cancellationToken = default);
}

using BeachRehberi.API.Models.Enums;

namespace BeachRehberi.API.Services;

public sealed class PaymentProcessResult
{
    public bool Success { get; init; }
    public bool RequiresRedirect { get; init; }
    public string? RedirectUrl { get; init; }
    public string? TransactionId { get; init; }
    public string? Message { get; init; }
    public PaymentStatus Status { get; init; } = PaymentStatus.Pending;

    public static PaymentProcessResult Failed(string message) => new()
    {
        Success = false,
        Message = message,
        Status = PaymentStatus.Failed
    };
}

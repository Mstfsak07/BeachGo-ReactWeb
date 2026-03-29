using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Enums;
using BeachRehberi.Domain.ValueObjects;

namespace BeachRehberi.Domain.Entities;

public class Subscription : BaseEntity
{
    public int TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public SubscriptionPlan Plan { get; private set; }
    public SubscriptionStatus Status { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "TRY";

    public string? StripePaymentIntentId { get; private set; }
    public string? StripeInvoiceId { get; private set; }

    // EF Core constructor
    private Subscription() { }

    public Subscription(int tenantId, SubscriptionPlan plan, DateTime startDate,
                        DateTime endDate, decimal amount, string currency = "TRY")
    {
        TenantId = tenantId;
        Plan = plan;
        Status = SubscriptionStatus.Active;
        StartDate = startDate;
        EndDate = endDate;
        Amount = amount;
        Currency = currency;
    }

    public void SetStripeIds(string paymentIntentId, string invoiceId)
    {
        StripePaymentIntentId = paymentIntentId;
        StripeInvoiceId = invoiceId;
        SetUpdated();
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
        SetUpdated();
    }

    public void MarkAsExpired()
    {
        Status = SubscriptionStatus.Expired;
        SetUpdated();
    }

    public bool IsActive() => Status == SubscriptionStatus.Active && EndDate > DateTime.UtcNow;
}

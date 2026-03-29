using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string ContactEmail { get; private set; } = string.Empty;
    public string ContactPhone { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public SubscriptionPlan Plan { get; private set; } = SubscriptionPlan.Free;
    public SubscriptionStatus SubscriptionStatus { get; private set; } = SubscriptionStatus.Active;
    public DateTime? SubscriptionExpiresAt { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public string? StripeSubscriptionId { get; private set; }

    // Plan limitleri
    public int MaxBeaches { get; private set; } = 1;
    public int MaxReservationsPerMonth { get; private set; } = 50;
    public bool CanUseAnalytics { get; private set; } = false;
    public bool CanUseApi { get; private set; } = false;

    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<Beach> Beaches { get; private set; } = new List<Beach>();
    public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();

    // EF Core constructor
    private Tenant() { }

    public Tenant(string name, string slug, string contactEmail)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        ContactEmail = contactEmail ?? throw new ArgumentNullException(nameof(contactEmail));
        ApplyPlanLimits(SubscriptionPlan.Free);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void UpgradePlan(SubscriptionPlan plan, DateTime expiresAt, string? stripeSubId = null)
    {
        Plan = plan;
        SubscriptionStatus = SubscriptionStatus.Active;
        SubscriptionExpiresAt = expiresAt;
        StripeSubscriptionId = stripeSubId;
        ApplyPlanLimits(plan);
        SetUpdated();
    }

    public void SetStripeCustomerId(string customerId)
    {
        StripeCustomerId = customerId;
        SetUpdated();
    }

    public bool IsSubscriptionActive()
    {
        if (SubscriptionStatus != SubscriptionStatus.Active) return false;
        if (Plan == SubscriptionPlan.Free) return true;
        return SubscriptionExpiresAt.HasValue && SubscriptionExpiresAt.Value > DateTime.UtcNow;
    }

    private void ApplyPlanLimits(SubscriptionPlan plan)
    {
        switch (plan)
        {
            case SubscriptionPlan.Free:
                MaxBeaches = 1;
                MaxReservationsPerMonth = 50;
                CanUseAnalytics = false;
                CanUseApi = false;
                break;
            case SubscriptionPlan.Pro:
                MaxBeaches = 5;
                MaxReservationsPerMonth = 500;
                CanUseAnalytics = true;
                CanUseApi = false;
                break;
            case SubscriptionPlan.Enterprise:
                MaxBeaches = 999;
                MaxReservationsPerMonth = 99999;
                CanUseAnalytics = true;
                CanUseApi = true;
                break;
        }
    }
}

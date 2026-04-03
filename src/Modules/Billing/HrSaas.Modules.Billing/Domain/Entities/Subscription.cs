using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;

namespace HrSaas.Modules.Billing.Domain.Entities;

public enum SubscriptionStatus { Active, PastDue, Cancelled, Trialing }

public sealed class Subscription : BaseEntity
{
    public string PlanName { get; private set; } = default!;
    public int MaxEmployees { get; private set; }
    public decimal MonthlyPrice { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime? TrialEndsAt { get; private set; }
    public DateTime? NextBillingDate { get; private set; }
    public string? ExternalSubscriptionId { get; private set; }

    private Subscription() { }

    public static Subscription CreateFree(Guid tenantId)
    {
        return new Subscription
        {
            TenantId = tenantId,
            PlanName = "Free",
            MaxEmployees = 10,
            MonthlyPrice = 0m,
            Status = SubscriptionStatus.Active
        };
    }

    public static Subscription CreateTrial(Guid tenantId, int trialDays = 14)
    {
        return new Subscription
        {
            TenantId = tenantId,
            PlanName = "Professional",
            MaxEmployees = 100,
            MonthlyPrice = 49.99m,
            Status = SubscriptionStatus.Trialing,
            TrialEndsAt = DateTime.UtcNow.AddDays(trialDays)
        };
    }

    public void Activate(string externalId, DateTime nextBillingDate)
    {
        Status = SubscriptionStatus.Active;
        ExternalSubscriptionId = externalId;
        NextBillingDate = nextBillingDate;
        Touch();
    }

    public void Cancel() { Status = SubscriptionStatus.Cancelled; Touch(); }

    public bool CanAddEmployee(int currentCount) => currentCount < MaxEmployees;
}

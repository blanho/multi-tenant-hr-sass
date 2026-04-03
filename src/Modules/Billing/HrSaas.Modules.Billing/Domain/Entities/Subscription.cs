using HrSaas.Modules.Billing.Domain.Events;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;

namespace HrSaas.Modules.Billing.Domain.Entities;

public enum SubscriptionStatus { Trial = 0, Active = 1, PastDue = 2, Cancelled = 3, Expired = 4 }
public enum BillingCycle { Monthly = 0, Annual = 1 }

public sealed class Subscription : BaseEntity
{
    public string PlanName { get; private set; } = null!;
    public SubscriptionStatus Status { get; private set; }
    public BillingCycle Cycle { get; private set; }
    public decimal PricePerCycle { get; private set; }
    public int MaxSeats { get; private set; }
    public int UsedSeats { get; private set; }
    public DateTime? TrialEndsAt { get; private set; }
    public DateTime? CurrentPeriodStart { get; private set; }
    public DateTime? CurrentPeriodEnd { get; private set; }
    public string? ExternalSubscriptionId { get; private set; }

    private Subscription() { }

    public static Subscription CreateFree(Guid tenantId)
    {
        var sub = new Subscription
        {
            TenantId = tenantId,
            PlanName = "Free",
            Status = SubscriptionStatus.Active,
            Cycle = BillingCycle.Monthly,
            PricePerCycle = 0,
            MaxSeats = 10,
            CurrentPeriodStart = DateTime.UtcNow
        };
        sub.AddDomainEvent(new SubscriptionCreatedEvent(tenantId, sub.Id, "Free"));
        return sub;
    }

    public static Subscription CreateTrial(Guid tenantId, string planName, int trialDays = 14)
    {
        var sub = new Subscription
        {
            TenantId = tenantId,
            PlanName = planName,
            Status = SubscriptionStatus.Trial,
            Cycle = BillingCycle.Monthly,
            PricePerCycle = 0,
            MaxSeats = 25,
            TrialEndsAt = DateTime.UtcNow.AddDays(trialDays)
        };
        sub.AddDomainEvent(new SubscriptionCreatedEvent(tenantId, sub.Id, planName));
        return sub;
    }

    public void Activate(decimal price, BillingCycle cycle, string? externalId = null)
    {
        Status = SubscriptionStatus.Active;
        PricePerCycle = price;
        Cycle = cycle;
        ExternalSubscriptionId = externalId;
        CurrentPeriodStart = DateTime.UtcNow;
        CurrentPeriodEnd = cycle == BillingCycle.Monthly
            ? DateTime.UtcNow.AddMonths(1)
            : DateTime.UtcNow.AddYears(1);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SubscriptionActivatedEvent(TenantId, Id, PlanName));
    }

    public void Cancel(string reason)
    {
        if (Status == SubscriptionStatus.Cancelled)
        {
            throw new DomainException("Subscription is already cancelled.");
        }

        Status = SubscriptionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SubscriptionCancelledEvent(TenantId, Id, reason));
    }

    public void MarkPastDue() { Status = SubscriptionStatus.PastDue; UpdatedAt = DateTime.UtcNow; }

    public bool CanAddSeat() => UsedSeats < MaxSeats;

    public void IncrementSeats()
    {
        if (!CanAddSeat())
        {
            throw new DomainException($"Seat limit of {MaxSeats} reached for plan {PlanName}.");
        }

        UsedSeats++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementSeats() { if (UsedSeats > 0) { UsedSeats--; UpdatedAt = DateTime.UtcNow; } }
}

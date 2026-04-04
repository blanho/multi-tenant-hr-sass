using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Billing.Domain.Events;

public sealed record SubscriptionCreatedEvent(
    Guid TenantId, Guid SubscriptionId, string PlanName, int MaxSeats, string BillingCycle) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record SubscriptionActivatedEvent(
    Guid TenantId, Guid SubscriptionId, string PlanName, decimal Price, string BillingCycle) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record SubscriptionCancelledEvent(Guid TenantId, Guid SubscriptionId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record SubscriptionPastDueEvent(Guid TenantId, Guid SubscriptionId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record SubscriptionExpiredEvent(Guid TenantId, Guid SubscriptionId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

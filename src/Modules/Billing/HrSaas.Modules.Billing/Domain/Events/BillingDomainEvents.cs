using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Billing.Domain.Events;

public sealed record SubscriptionCreatedEvent(Guid TenantId, Guid SubscriptionId, string PlanName) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record SubscriptionActivatedEvent(Guid TenantId, Guid SubscriptionId, string PlanName) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record SubscriptionCancelledEvent(Guid TenantId, Guid SubscriptionId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

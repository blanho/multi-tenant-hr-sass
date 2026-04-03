namespace HrSaas.Contracts.Billing;

using HrSaas.SharedKernel.Events;

public sealed record SubscriptionCreatedIntegrationEvent(
    Guid TenantId,
    Guid SubscriptionId,
    string PlanName,
    int MaxSeats,
    string BillingCycle) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record SubscriptionActivatedIntegrationEvent(
    Guid TenantId,
    Guid SubscriptionId,
    string PlanName,
    decimal Price,
    string BillingCycle) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record SubscriptionCancelledIntegrationEvent(
    Guid TenantId,
    Guid SubscriptionId,
    string Reason) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record SubscriptionPastDueIntegrationEvent(
    Guid TenantId,
    Guid SubscriptionId) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

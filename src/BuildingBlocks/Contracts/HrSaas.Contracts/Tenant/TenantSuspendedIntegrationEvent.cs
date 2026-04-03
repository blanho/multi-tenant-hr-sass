namespace HrSaas.Contracts.Tenant;

using HrSaas.SharedKernel.Events;

public sealed record TenantSuspendedIntegrationEvent(
    Guid TenantId,
    string Reason) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record TenantReactivatedIntegrationEvent(
    Guid TenantId) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

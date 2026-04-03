using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Tenant.Domain.Events;

public sealed record TenantCreatedIntegrationEvent(
    Guid TenantId,
    string Name,
    string Slug,
    string Plan) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record TenantSuspendedIntegrationEvent(
    Guid TenantId,
    string Reason) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record TenantPlanUpgradedIntegrationEvent(
    Guid TenantId,
    string OldPlan,
    string NewPlan) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

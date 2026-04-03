using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Tenant.Domain.Events;

public sealed record TenantCreatedEvent(Guid TenantId, string Name, string Plan) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record TenantPlanUpgradedEvent(Guid TenantId, string OldPlan, string NewPlan) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record TenantSuspendedEvent(Guid TenantId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

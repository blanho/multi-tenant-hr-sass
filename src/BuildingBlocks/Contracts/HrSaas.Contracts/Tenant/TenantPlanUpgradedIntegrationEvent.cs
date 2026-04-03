namespace HrSaas.Contracts.Tenant;

using HrSaas.SharedKernel.Events;

public sealed record TenantPlanUpgradedIntegrationEvent(
    Guid TenantId,
    string OldPlan,
    string NewPlan,
    int NewMaxSeats) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

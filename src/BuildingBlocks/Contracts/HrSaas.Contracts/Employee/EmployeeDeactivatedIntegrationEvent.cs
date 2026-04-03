namespace HrSaas.Contracts.Employee;

using HrSaas.SharedKernel.Events;

public sealed record EmployeeDeactivatedIntegrationEvent(
    Guid TenantId,
    Guid EmployeeId) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

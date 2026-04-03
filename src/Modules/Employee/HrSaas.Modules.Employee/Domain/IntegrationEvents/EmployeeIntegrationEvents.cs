using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Employee.Domain.IntegrationEvents;

public record EmployeeCreatedIntegrationEvent(
    Guid TenantId,
    Guid EmployeeId,
    string Name,
    string Department) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record EmployeeDeletedIntegrationEvent(
    Guid TenantId,
    Guid EmployeeId) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

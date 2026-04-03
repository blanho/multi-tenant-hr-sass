using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Employee.Domain.IntegrationEvents;

public sealed record EmployeeCreatedIntegrationEvent(
    Guid TenantId,
    Guid EmployeeId,
    string Name,
    string Department) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record EmployeeUpdatedIntegrationEvent(
    Guid TenantId,
    Guid EmployeeId,
    string Name,
    string Department,
    string Position) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record EmployeeDeletedIntegrationEvent(
    Guid TenantId,
    Guid EmployeeId) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

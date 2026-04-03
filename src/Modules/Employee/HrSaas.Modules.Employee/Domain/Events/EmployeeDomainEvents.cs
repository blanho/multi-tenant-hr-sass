using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Employee.Domain.Events;

public record EmployeeCreatedEvent(
    Guid TenantId,
    Guid EmployeeId,
    string Name,
    string Department,
    string Position) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record EmployeeUpdatedEvent(
    Guid TenantId,
    Guid EmployeeId,
    string Name) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record EmployeeDeletedEvent(
    Guid TenantId,
    Guid EmployeeId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

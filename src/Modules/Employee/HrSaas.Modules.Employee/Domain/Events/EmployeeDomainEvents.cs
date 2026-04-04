using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Employee.Domain.Events;

public sealed record EmployeeCreatedEvent(
    Guid TenantId,
    Guid EmployeeId,
    string Name,
    string Department,
    string Position,
    string Email) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record EmployeeUpdatedEvent(
    Guid TenantId,
    Guid EmployeeId,
    string Name,
    string Department,
    string Position) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record EmployeeDeactivatedEvent(
    Guid TenantId,
    Guid EmployeeId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record EmployeeDeletedEvent(
    Guid TenantId,
    Guid EmployeeId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record EmployeeReinstatedEvent(
    Guid TenantId,
    Guid EmployeeId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

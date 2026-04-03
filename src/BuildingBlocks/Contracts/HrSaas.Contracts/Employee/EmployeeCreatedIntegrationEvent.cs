namespace HrSaas.Contracts.Employee;

using HrSaas.SharedKernel.Events;

public sealed record EmployeeCreatedIntegrationEvent(
    Guid TenantId,
    Guid EmployeeId,
    string Name,
    string Department,
    string Position,
    string Email) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

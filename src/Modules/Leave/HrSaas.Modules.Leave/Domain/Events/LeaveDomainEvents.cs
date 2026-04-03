using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Leave.Domain.Events;

public record LeaveApprovedEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record LeaveRejectedEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

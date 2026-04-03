using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Leave.Domain.Events;

public sealed record LeaveAppliedEvent(Guid TenantId, Guid LeaveRequestId, Guid EmployeeId, string LeaveType) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record LeaveApprovedEvent(Guid TenantId, Guid LeaveRequestId, Guid EmployeeId, Guid ApprovedBy) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record LeaveRejectedEvent(Guid TenantId, Guid LeaveRequestId, Guid EmployeeId, string Note) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

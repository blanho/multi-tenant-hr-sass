using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Leave.Domain.Events;

public sealed record LeaveAppliedIntegrationEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId,
    string LeaveType,
    DateTime StartDate,
    DateTime EndDate) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record LeaveApprovedIntegrationEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId,
    Guid ApprovedByUserId) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record LeaveRejectedIntegrationEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record LeaveCancelledIntegrationEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

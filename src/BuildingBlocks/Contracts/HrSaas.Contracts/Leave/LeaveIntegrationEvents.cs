namespace HrSaas.Contracts.Leave;

using HrSaas.SharedKernel.Events;

public sealed record LeaveAppliedIntegrationEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId,
    string LeaveType,
    DateTime StartDate,
    DateTime EndDate,
    int DurationDays,
    string Reason) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record LeaveApprovedIntegrationEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId,
    Guid ApprovedByUserId,
    string LeaveType,
    int DurationDays) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record LeaveRejectedIntegrationEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId,
    string RejectionNote) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record LeaveCancelledIntegrationEvent(
    Guid TenantId,
    Guid LeaveRequestId,
    Guid EmployeeId) : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

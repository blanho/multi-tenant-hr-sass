namespace HrSaas.Modules.Leave.Application.DTOs;

public sealed record LeaveRequestDto(
    Guid Id,
    Guid TenantId,
    Guid EmployeeId,
    string Type,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    string Reason,
    string? RejectionNote,
    int DurationDays,
    DateTime CreatedAt);

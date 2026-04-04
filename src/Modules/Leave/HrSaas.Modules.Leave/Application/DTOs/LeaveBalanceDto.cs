namespace HrSaas.Modules.Leave.Application.DTOs;

public sealed record LeaveBalanceDto(
    Guid Id,
    Guid EmployeeId,
    int Year,
    int AnnualAllowance,
    int SickAllowance,
    int AnnualUsed,
    int SickUsed,
    int AnnualRemaining,
    int SickRemaining);

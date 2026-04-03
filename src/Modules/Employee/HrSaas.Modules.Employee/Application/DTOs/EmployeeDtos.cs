namespace HrSaas.Modules.Employee.Application.DTOs;

public record EmployeeDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Department,
    string Position,
    string Email,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record EmployeeSummaryDto(
    Guid Id,
    string Name,
    string Department,
    string Position);

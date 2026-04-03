namespace HrSaas.Modules.Tenant.Application.DTOs;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string ContactEmail,
    string Plan,
    string Status,
    int MaxEmployees,
    DateTime CreatedAt);

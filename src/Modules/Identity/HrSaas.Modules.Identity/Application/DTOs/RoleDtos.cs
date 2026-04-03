namespace HrSaas.Modules.Identity.Application.DTOs;

public sealed record RoleDto(
    Guid Id,
    Guid TenantId,
    string Name,
    bool IsSystemRole,
    IReadOnlyList<string> Permissions,
    DateTime CreatedAt);

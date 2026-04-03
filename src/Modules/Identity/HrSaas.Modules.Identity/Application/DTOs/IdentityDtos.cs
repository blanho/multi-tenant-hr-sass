using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;

namespace HrSaas.Modules.Identity.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt);

public sealed record AuthTokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

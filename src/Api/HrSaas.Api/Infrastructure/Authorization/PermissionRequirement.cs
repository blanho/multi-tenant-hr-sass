using Microsoft.AspNetCore.Authorization;

namespace HrSaas.Api.Infrastructure.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

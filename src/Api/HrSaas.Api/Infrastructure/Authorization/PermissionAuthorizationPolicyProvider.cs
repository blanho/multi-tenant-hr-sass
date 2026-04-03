using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HrSaas.Api.Infrastructure.Authorization;

public sealed class PermissionAuthorizationPolicyProvider(
    IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(HasPermissionAttribute.PolicyPrefix, StringComparison.Ordinal))
            return await base.GetPolicyAsync(policyName).ConfigureAwait(false);

        var permission = policyName[HasPermissionAttribute.PolicyPrefix.Length..];

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();
    }
}

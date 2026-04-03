using Microsoft.AspNetCore.Authorization;

namespace HrSaas.Api.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "PERMISSION_";

    public HasPermissionAttribute(string permission)
        : base($"{PolicyPrefix}{permission}")
    {
    }
}

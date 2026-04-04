using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HrSaas.TenantSdk;

public sealed class TenantMiddleware(
    RequestDelegate next,
    ILogger<TenantMiddleware> logger)
{
    private const string TenantIdClaimType = "tenant_id";
    private const string TenantIdHeaderName = "X-Tenant-ID";

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        if (ShouldSkip(context))
        {
            await next(context);
            return;
        }

        var tenantId = ExtractTenantId(context);

        if (tenantId is null)
        {
            logger.LogWarning(
                "Request to {Path} rejected: missing tenant_id claim or X-Tenant-ID header",
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7807",


                title = "Tenant identification required",
                status = 401,
                detail = "Request must include a valid tenant_id claim in the JWT or X-Tenant-ID header."
            });
            return;
        }

        tenantService.SetCurrentTenant(tenantId.Value);

        logger.LogDebug(
            "Tenant resolved: {TenantId} for {Path}",
            tenantId.Value,
            context.Request.Path);

        await next(context);
    }

    private static Guid? ExtractTenantId(HttpContext context)
    {
        var claim = context.User.FindFirst(TenantIdClaimType)?.Value;
        if (!string.IsNullOrWhiteSpace(claim) && Guid.TryParse(claim, out var jwtTenantId))
            return jwtTenantId;

        var header = context.Request.Headers[TenantIdHeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(header) && Guid.TryParse(header, out var headerTenantId))
            return headerTenantId;

        return null;
    }

    private static bool ShouldSkip(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        return path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/v1/auth/register", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase);
    }
}

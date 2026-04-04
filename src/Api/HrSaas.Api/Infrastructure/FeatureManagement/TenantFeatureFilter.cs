using Microsoft.FeatureManagement;

namespace HrSaas.Api.Infrastructure.FeatureManagement;

[FilterAlias("Tenant")]
public sealed class TenantFeatureFilter(IHttpContextAccessor httpContextAccessor) : IFeatureFilter
{
    private const string TenantIdClaimType = "tenant_id";
    private const string TenantIdHeaderName = "X-Tenant-ID";

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var tenantId = ResolveTenantId();
        if (tenantId is null)
            return Task.FromResult(false);

        var settings = context.Parameters.Get<TenantFilterSettings>();
        if (settings is null)
            return Task.FromResult(false);

        if (settings.AllowedTenants is not null &&
            settings.AllowedTenants.Contains(tenantId.Value.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(true);
        }

        if (settings.DisallowedTenants is not null &&
            settings.DisallowedTenants.Contains(tenantId.Value.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(settings.DefaultEnabled);
    }

    private Guid? ResolveTenantId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return null;

        var claim = httpContext.User.FindFirst(TenantIdClaimType)?.Value;
        if (!string.IsNullOrWhiteSpace(claim) && Guid.TryParse(claim, out var jwtTenantId))
            return jwtTenantId;

        var header = httpContext.Request.Headers[TenantIdHeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(header) && Guid.TryParse(header, out var headerTenantId))
            return headerTenantId;

        return null;
    }
}

public sealed class TenantFilterSettings
{
    public bool DefaultEnabled { get; set; }
    public IList<string>? AllowedTenants { get; set; }
    public IList<string>? DisallowedTenants { get; set; }
}

namespace HrSaas.SharedKernel.FeatureFlags;

public sealed class TenantFilterSettings
{
    public bool DefaultEnabled { get; set; }
    public IList<string>? AllowedTenants { get; set; }
    public IList<string>? DisallowedTenants { get; set; }
}

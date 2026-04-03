namespace HrSaas.TenantSdk;

public sealed class TenantContext
{
    public Guid TenantId { get; set; } = Guid.Empty;

    public bool IsSet => TenantId != Guid.Empty;
}

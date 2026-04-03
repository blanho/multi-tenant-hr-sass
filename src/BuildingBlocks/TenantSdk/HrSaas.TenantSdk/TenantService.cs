using HrSaas.SharedKernel.Exceptions;

namespace HrSaas.TenantSdk;

public sealed class TenantService(TenantContext context) : ITenantService
{
    public Guid GetCurrentTenantId()
    {
        if (!context.IsSet)
            throw new TenantNotFoundException("Tenant context has not been set for this request.");

        return context.TenantId;
    }

    public void SetCurrentTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));

        context.TenantId = tenantId;
    }

    public bool HasTenant() => context.IsSet;
}

namespace HrSaas.TenantSdk;

public interface ITenantService
{
    Guid GetCurrentTenantId();

    void SetCurrentTenant(Guid tenantId);

    bool HasTenant();
}

using TenantEntity = HrSaas.Modules.Tenant.Domain.Entities.Tenant;

namespace HrSaas.Modules.Tenant.Application.Interfaces;

public interface ITenantRepository
{
    Task<TenantEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TenantEntity?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<TenantEntity>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TenantEntity tenant, CancellationToken ct = default);
    void Update(TenantEntity tenant);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

using HrSaas.Modules.Tenant.Application.Interfaces;
using HrSaas.Modules.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Tenant.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(TenantDbContext dbContext) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct).ConfigureAwait(false);

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug && !t.IsDeleted, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default) =>
        await dbContext.Tenants.Where(t => !t.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default) =>
        await dbContext.Tenants.AddAsync(tenant, ct).ConfigureAwait(false);

    public void Update(Tenant tenant) => dbContext.Tenants.Update(tenant);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}

using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Identity.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(IdentityDbContext dbContext) : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id, ct).ConfigureAwait(false);

    public async Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken ct = default) =>
        await dbContext.Roles
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == name, ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Role>> GetAllAsync(Guid tenantId, CancellationToken ct = default) =>
        await dbContext.Roles
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Name)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task AddAsync(Role role, CancellationToken ct = default) =>
        await dbContext.Roles.AddAsync(role, ct).ConfigureAwait(false);

    public async Task AddRangeAsync(IEnumerable<Role> roles, CancellationToken ct = default) =>
        await dbContext.Roles.AddRangeAsync(roles, ct).ConfigureAwait(false);

    public void Update(Role role) => dbContext.Roles.Update(role);

    public void Delete(Role role) => role.Delete();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}

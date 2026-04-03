using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, ct).ConfigureAwait(false);

    public async Task<AppUser?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default) =>
        await dbContext.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email.Value == email.ToLowerInvariant(), ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<AppUser>> GetAllAsync(Guid tenantId, CancellationToken ct = default) =>
        await dbContext.Users.Where(u => u.TenantId == tenantId && !u.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task AddAsync(AppUser user, CancellationToken ct = default) =>
        await dbContext.Users.AddAsync(user, ct).ConfigureAwait(false);

    public void Update(AppUser user) => dbContext.Users.Update(user);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}

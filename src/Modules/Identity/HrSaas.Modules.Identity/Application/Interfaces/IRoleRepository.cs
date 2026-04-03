using HrSaas.Modules.Identity.Domain.Entities;

namespace HrSaas.Modules.Identity.Application.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Role role, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Role> roles, CancellationToken ct = default);
    void Update(Role role);
    void Delete(Role role);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

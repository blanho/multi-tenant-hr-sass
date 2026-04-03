using HrSaas.Modules.Identity.Domain.Entities;

namespace HrSaas.Modules.Identity.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AppUser?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<IReadOnlyList<AppUser>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(AppUser user, CancellationToken ct = default);
    void Update(AppUser user);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IPasswordHasher
{
    string Hash(string plainText);
    bool Verify(string plainText, string hash);
}

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, Guid tenantId, string email, string role);
    string GenerateRefreshToken();
    (Guid UserId, Guid TenantId, string Role) ValidateRefreshToken(string token);
}

using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;

namespace HrSaas.Modules.Identity.Domain.Entities;

public sealed class AppUser : BaseEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private AppUser() { }

    public static AppUser Create(Guid tenantId, string email, string passwordHash, string role)
    {
        if (tenantId == Guid.Empty) throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required.");
        if (!IsValidRole(role)) throw new DomainException($"Role '{role}' is not valid.");

        return new AppUser
        {
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public void ChangeRole(string newRole)
    {
        if (!IsValidRole(newRole)) throw new DomainException($"Role '{newRole}' is not valid.");
        Role = newRole;
        Touch();
    }

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }

    private static bool IsValidRole(string role) =>
        role is "Admin" or "Manager" or "Employee";
}

using HrSaas.Modules.Identity.Domain.Events;
using HrSaas.Modules.Identity.Domain.ValueObjects;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Identity.Domain.Entities;

public sealed class AppUser : BaseEntity
{
    public static readonly IReadOnlyList<string> AllowedRoles = ["Admin", "Manager", "Employee"];

    public Email Email { get; private set; } = null!;
    public HashedPassword Password { get; private set; } = null!;
    public string Role { get; private set; } = null!;

    private AppUser() { }

    public static AppUser Create(Guid tenantId, Email email, HashedPassword password, string role)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(email, nameof(email));
        Guard.NotNull(password, nameof(password));
        Guard.NotNullOrWhiteSpace(role, nameof(role));

        var user = new AppUser
        {
            TenantId = tenantId,
            Email = email,
            Password = password,
            Role = role
        };

        user.AddDomainEvent(new UserRegisteredEvent(tenantId, user.Id, email.Value));
        return user;
    }

    public void ChangeRole(string newRole)
    {
        Guard.NotNullOrWhiteSpace(newRole, nameof(newRole));
        var old = Role;
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserRoleChangedEvent(TenantId, Id, old, newRole));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserDeactivatedEvent(TenantId, Id));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

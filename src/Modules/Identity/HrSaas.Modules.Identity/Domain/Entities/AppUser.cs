using HrSaas.Modules.Identity.Domain.Events;
using HrSaas.Modules.Identity.Domain.ValueObjects;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Identity.Domain.Entities;

public sealed class AppUser : BaseEntity
{
    public Email Email { get; private set; } = null!;
    public HashedPassword Password { get; private set; } = null!;
    public Guid RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AppUser() { }

    public static AppUser Create(Guid tenantId, Email email, HashedPassword password, Guid roleId)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(email, nameof(email));
        Guard.NotNull(password, nameof(password));
        Guard.NotEmpty(roleId, nameof(roleId));

        var user = new AppUser
        {
            TenantId = tenantId,
            Email = email,
            Password = password,
            RoleId = roleId
        };

        user.AddDomainEvent(new UserRegisteredEvent(tenantId, user.Id, email.Value));
        return user;
    }

    public void AssignRole(Guid oldRoleId, Guid newRoleId)
    {
        Guard.NotEmpty(newRoleId, nameof(newRoleId));
        RoleId = newRoleId;
        Touch();
        AddDomainEvent(new UserRoleChangedEvent(TenantId, Id, oldRoleId, newRoleId));
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
        AddDomainEvent(new UserDeactivatedEvent(TenantId, Id));
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }
}

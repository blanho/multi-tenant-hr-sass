using HrSaas.Modules.Identity.Domain.Events;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Identity.Domain.Entities;

public sealed class Role : BaseEntity
{
    public string Name { get; private set; } = null!;
    public bool IsSystemRole { get; private set; }
    public List<string> Permissions { get; private set; } = [];

    private Role() { }

    public static Role Create(Guid tenantId, string name, bool isSystemRole, IEnumerable<string> permissions)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        Guard.NotNullOrWhiteSpace(name, nameof(name));

        var permissionList = permissions.ToList();
        ValidatePermissions(permissionList);

        var role = new Role
        {
            TenantId = tenantId,
            Name = name,
            IsSystemRole = isSystemRole,
            Permissions = permissionList,
        };

        role.AddDomainEvent(new RoleCreatedEvent(tenantId, role.Id, name));
        return role;
    }

    public void UpdatePermissions(IEnumerable<string> newPermissions)
    {
        var permissionList = newPermissions.ToList();
        ValidatePermissions(permissionList);

        Permissions = permissionList;
        Touch();
        AddDomainEvent(new RolePermissionsChangedEvent(TenantId, Id, Name, permissionList));
    }

    public void Rename(string newName)
    {
        Guard.NotNullOrWhiteSpace(newName, nameof(newName));
        Name = newName;
        Touch();
    }

    public bool HasPermission(string permission) => Permissions.Contains(permission);

    private static void ValidatePermissions(List<string> permissions)
    {
        var invalid = permissions.Where(p => !Permission.IsValid(p)).ToList();
        if (invalid.Count > 0)
            throw new ArgumentException($"Invalid permissions: {string.Join(", ", invalid)}");
    }
}

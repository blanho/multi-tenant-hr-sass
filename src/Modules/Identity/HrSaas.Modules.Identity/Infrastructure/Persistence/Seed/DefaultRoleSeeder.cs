using HrSaas.Modules.Identity.Domain.Entities;

namespace HrSaas.Modules.Identity.Infrastructure.Persistence.Seed;

public static class DefaultRoleSeeder
{
    public static IReadOnlyList<Role> CreateDefaultRoles(Guid tenantId) =>
    [
        Role.Create(tenantId, "Admin", isSystemRole: true, Permission.All),
        Role.Create(tenantId, "Manager", isSystemRole: true,
        [
            Permission.Employees.View,
            Permission.Employees.Create,
            Permission.Employees.Update,
            Permission.Leaves.View,
            Permission.Leaves.Approve,
            Permission.Leaves.Reject,
            Permission.Users.View,
            Permission.Billing.View,
        ]),
        Role.Create(tenantId, "Employee", isSystemRole: true,
        [
            Permission.Employees.View,
            Permission.Leaves.View,
            Permission.Leaves.Create,
            Permission.Leaves.Cancel,
        ]),
    ];
}

namespace HrSaas.Modules.Identity.Domain.Entities;

public static class Permission
{
    public static class Employees
    {
        public const string View = "employees.view";
        public const string Create = "employees.create";
        public const string Update = "employees.update";
        public const string Delete = "employees.delete";

        public static readonly IReadOnlyList<string> All = [View, Create, Update, Delete];
    }

    public static class Leaves
    {
        public const string View = "leaves.view";
        public const string Create = "leaves.create";
        public const string Approve = "leaves.approve";
        public const string Reject = "leaves.reject";
        public const string Cancel = "leaves.cancel";

        public static readonly IReadOnlyList<string> All = [View, Create, Approve, Reject, Cancel];
    }

    public static class Tenants
    {
        public const string View = "tenants.view";
        public const string Create = "tenants.create";
        public const string Suspend = "tenants.suspend";
        public const string Reinstate = "tenants.reinstate";
        public const string UpgradePlan = "tenants.upgrade-plan";

        public static readonly IReadOnlyList<string> All = [View, Create, Suspend, Reinstate, UpgradePlan];
    }

    public static class Billing
    {
        public const string View = "billing.view";
        public const string Cancel = "billing.cancel";

        public static readonly IReadOnlyList<string> All = [View, Cancel];
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string AssignRole = "users.assign-role";
        public const string Deactivate = "users.deactivate";

        public static readonly IReadOnlyList<string> All = [View, Create, AssignRole, Deactivate];
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";

        public static readonly IReadOnlyList<string> All = [View, Create, Update, Delete];
    }

    public static readonly IReadOnlyList<string> All =
    [
        ..Employees.All,
        ..Leaves.All,
        ..Tenants.All,
        ..Billing.All,
        ..Users.All,
        ..Roles.All,
    ];

    public static bool IsValid(string permission) => All.Contains(permission);
}

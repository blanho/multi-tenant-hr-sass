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

    public static class Notifications
    {
        public const string View = "notifications.view";
        public const string Send = "notifications.send";
        public const string Manage = "notifications.manage";

        public static readonly IReadOnlyList<string> All = [View, Send, Manage];
    }

    public static class Files
    {
        public const string View = "files.view";
        public const string Upload = "files.upload";
        public const string Download = "files.download";
        public const string Delete = "files.delete";

        public static readonly IReadOnlyList<string> All = [View, Upload, Download, Delete];
    }

    public static class Audit
    {
        public const string View = "audit.view";
        public const string Export = "audit.export";

        public static readonly IReadOnlyList<string> All = [View, Export];
    }

    public static class Reports
    {
        public const string View = "reports.view";
        public const string Export = "reports.export";

        public static readonly IReadOnlyList<string> All = [View, Export];
    }

    public static readonly IReadOnlyList<string> All =
    [
        ..Employees.All,
        ..Leaves.All,
        ..Tenants.All,
        ..Billing.All,
        ..Users.All,
        ..Roles.All,
        ..Notifications.All,
        ..Files.All,
        ..Audit.All,
        ..Reports.All,
    ];

    public static bool IsValid(string permission) => All.Contains(permission);
}

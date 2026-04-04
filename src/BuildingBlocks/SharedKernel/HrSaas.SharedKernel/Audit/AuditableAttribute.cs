namespace HrSaas.SharedKernel.Audit;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuditableAttribute : Attribute
{
    public AuditAction Action { get; }

    public AuditCategory Category { get; }

    public AuditSeverity Severity { get; init; } = AuditSeverity.Medium;

    public string? Description { get; init; }

    public AuditableAttribute(AuditAction action, AuditCategory category)
    {
        Action = action;
        Category = category;
    }
}

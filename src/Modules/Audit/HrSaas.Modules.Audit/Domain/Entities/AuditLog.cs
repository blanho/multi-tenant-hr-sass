using HrSaas.SharedKernel.Audit;

namespace HrSaas.Modules.Audit.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public Guid? UserId { get; private set; }

    public string? UserEmail { get; private set; }

    public AuditAction Action { get; private set; }

    public AuditCategory Category { get; private set; }

    public AuditSeverity Severity { get; private set; }

    public string EntityType { get; private set; } = string.Empty;

    public string? EntityId { get; private set; }

    public string CommandName { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Payload { get; private set; }

    public string? OldValues { get; private set; }

    public string? NewValues { get; private set; }

    public bool IsSuccess { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public string? CorrelationId { get; private set; }

    public long DurationMs { get; private set; }

    public DateTime Timestamp { get; private set; }

    private AuditLog() { }

    public static AuditLog FromEntry(AuditEntry entry) => new()
    {
        Id = entry.Id,
        TenantId = entry.TenantId,
        UserId = entry.UserId,
        UserEmail = entry.UserEmail,
        Action = entry.Action,
        Category = entry.Category,
        Severity = entry.Severity,
        EntityType = entry.EntityType,
        EntityId = entry.EntityId,
        CommandName = entry.CommandName,
        Description = entry.Description,
        Payload = entry.Payload?.RootElement.GetRawText(),
        OldValues = entry.OldValues?.RootElement.GetRawText(),
        NewValues = entry.NewValues?.RootElement.GetRawText(),
        IsSuccess = entry.IsSuccess,
        ErrorMessage = entry.ErrorMessage,
        IpAddress = entry.IpAddress,
        UserAgent = entry.UserAgent,
        CorrelationId = entry.CorrelationId,
        DurationMs = entry.DurationMs,
        Timestamp = entry.Timestamp
    };
}

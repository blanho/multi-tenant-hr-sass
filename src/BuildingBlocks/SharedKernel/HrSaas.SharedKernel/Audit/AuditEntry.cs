using System.Text.Json;

namespace HrSaas.SharedKernel.Audit;

public sealed class AuditEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid TenantId { get; init; }

    public Guid? UserId { get; init; }

    public string? UserEmail { get; init; }

    public AuditAction Action { get; init; }

    public AuditCategory Category { get; init; }

    public AuditSeverity Severity { get; init; }

    public string EntityType { get; init; } = string.Empty;

    public string? EntityId { get; init; }

    public string CommandName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public JsonDocument? Payload { get; init; }

    public JsonDocument? OldValues { get; init; }

    public JsonDocument? NewValues { get; init; }

    public bool IsSuccess { get; init; }

    public string? ErrorMessage { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public string? CorrelationId { get; init; }

    public long DurationMs { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

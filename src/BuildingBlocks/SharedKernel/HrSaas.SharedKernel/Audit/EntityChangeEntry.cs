using System.Text.Json;

namespace HrSaas.SharedKernel.Audit;

public sealed class EntityChangeEntry
{
    public string EntityType { get; init; } = string.Empty;

    public string? EntityId { get; init; }

    public AuditAction Action { get; init; }

    public JsonDocument? OldValues { get; init; }

    public JsonDocument? NewValues { get; init; }

    public IReadOnlyList<string> ChangedProperties { get; init; } = [];
}

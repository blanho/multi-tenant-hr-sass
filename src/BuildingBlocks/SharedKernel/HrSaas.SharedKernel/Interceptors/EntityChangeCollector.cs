using System.Text.Json;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HrSaas.SharedKernel.Interceptors;

public sealed class EntityChangeCollector : IEntityChangeCollector
{
    private readonly List<EntityChangeEntry> _entries = [];

    public IReadOnlyList<EntityChangeEntry> Collect() => _entries.AsReadOnly();

    public void Clear() => _entries.Clear();

    public void CaptureChanges(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            var change = entry.State switch
            {
                EntityState.Added => CaptureAdded(entry),
                EntityState.Modified => CaptureModified(entry),
                EntityState.Deleted => CaptureDeleted(entry),
                _ => null
            };

            if (change is not null)
            {
                _entries.Add(change);
            }
        }
    }

    private static EntityChangeEntry CaptureAdded(EntityEntry<BaseEntity> entry)
    {
        var newValues = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties.Where(p => p.CurrentValue is not null))
        {
            newValues[prop.Metadata.Name] = prop.CurrentValue;
        }

        return new EntityChangeEntry
        {
            EntityType = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id.ToString(),
            Action = AuditAction.Create,
            NewValues = SerializeDict(newValues)
        };
    }

    private static EntityChangeEntry CaptureModified(EntityEntry<BaseEntity> entry)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        var changed = new List<string>();

        foreach (var prop in entry.Properties)
        {
            if (!prop.IsModified)
            {
                continue;
            }

            var propertyName = prop.Metadata.Name;
            oldValues[propertyName] = prop.OriginalValue;
            newValues[propertyName] = prop.CurrentValue;
            changed.Add(propertyName);
        }

        return new EntityChangeEntry
        {
            EntityType = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id.ToString(),
            Action = AuditAction.Update,
            OldValues = SerializeDict(oldValues),
            NewValues = SerializeDict(newValues),
            ChangedProperties = changed
        };
    }

    private static EntityChangeEntry CaptureDeleted(EntityEntry<BaseEntity> entry)
    {
        var oldValues = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties.Where(p => p.OriginalValue is not null))
        {
            oldValues[prop.Metadata.Name] = prop.OriginalValue;
        }

        return new EntityChangeEntry
        {
            EntityType = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id.ToString(),
            Action = AuditAction.Delete,
            OldValues = SerializeDict(oldValues)
        };
    }

    private static JsonDocument? SerializeDict(Dictionary<string, object?> dict)
    {
        if (dict.Count == 0)
        {
            return null;
        }

        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        return JsonDocument.Parse(json);
    }
}

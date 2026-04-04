using HrSaas.Modules.Audit.Domain.Entities;
using HrSaas.Modules.Audit.Infrastructure.Persistence;
using HrSaas.SharedKernel.Audit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Audit.Infrastructure;

public sealed class EfAuditLogStore(
    AuditDbContext dbContext,
    ILogger<EfAuditLogStore> logger) : IAuditLogStore
{
    public async Task StoreAsync(AuditEntry entry, CancellationToken ct = default)
    {
        try
        {
            var entity = AuditLog.FromEntry(entry);
            dbContext.AuditLogs.Add(entity);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to store audit log for {CommandName}", entry.CommandName);
        }
    }

    public async Task StoreBatchAsync(IReadOnlyList<AuditEntry> entries, CancellationToken ct = default)
    {
        try
        {
            var entities = entries.Select(AuditLog.FromEntry).ToList();
            dbContext.AuditLogs.AddRange(entities);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to store {Count} audit log entries", entries.Count);
        }
    }
}

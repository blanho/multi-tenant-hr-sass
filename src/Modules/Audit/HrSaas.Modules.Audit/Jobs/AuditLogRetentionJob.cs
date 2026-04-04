using HrSaas.Modules.Audit.Infrastructure.Persistence;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Audit.Jobs;

public sealed class AuditLogRetentionJob(
    AuditDbContext dbContext,
    ILogger<AuditLogRetentionJob> logger) : IRecurringJob
{
    private const int RetentionDays = 365;
    private const int BatchSize = 1000;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-RetentionDays);

        var deleted = await dbContext.AuditLogs
            .Where(a => a.Timestamp < cutoff)
            .OrderBy(a => a.Timestamp)
            .Take(BatchSize)
            .ExecuteDeleteAsync(ct)
            .ConfigureAwait(false);

        if (deleted > 0)
        {
            logger.LogInformation(
                "Audit log retention: deleted {Count} entries older than {CutoffDate}",
                deleted, cutoff);
        }
    }
}

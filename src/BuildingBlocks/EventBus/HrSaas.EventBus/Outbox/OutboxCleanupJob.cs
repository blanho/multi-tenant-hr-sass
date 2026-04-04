using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrSaas.EventBus.Outbox;

public sealed class OutboxCleanupJob(
    OutboxDbContext dbContext,
    ILogger<OutboxCleanupJob> logger) : IRecurringJob
{
    private const int RetentionDays = 7;
    private const int BatchSize = 500;

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-RetentionDays);

        var deletedCount = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt.HasValue && m.ProcessedAt < cutoff)
            .OrderBy(m => m.ProcessedAt)
            .Take(BatchSize)
            .ExecuteDeleteAsync(ct)
            .ConfigureAwait(false);

        if (deletedCount > 0)
        {
            logger.LogInformation(
                "Cleaned up {Count} processed outbox messages older than {RetentionDays} days",
                deletedCount, RetentionDays);
        }
    }
}

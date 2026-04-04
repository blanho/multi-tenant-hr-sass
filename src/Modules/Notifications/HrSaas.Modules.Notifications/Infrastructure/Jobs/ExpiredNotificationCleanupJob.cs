using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Infrastructure.Persistence;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Infrastructure.Jobs;

public sealed class ExpiredNotificationCleanupJob(
    NotificationsDbContext dbContext,
    ILogger<ExpiredNotificationCleanupJob> logger) : IRecurringJob
{
    private const int BatchSize = 200;

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var cancelledCount = await dbContext.Notifications
            .IgnoreQueryFilters()
            .Where(n => n.ExpiresAt.HasValue
                && n.ExpiresAt < now
                && n.Status != NotificationStatus.Cancelled
                && n.Status != NotificationStatus.Delivered
                && n.Status != NotificationStatus.Read)
            .Take(BatchSize)
            .ExecuteUpdateAsync(
                setter => setter
                    .SetProperty(n => n.Status, NotificationStatus.Cancelled)
                    .SetProperty(n => n.UpdatedAt, now),
                ct)
            .ConfigureAwait(false);

        var retentionCutoff = now.AddDays(-90);

        var purgedCount = await dbContext.Notifications
            .IgnoreQueryFilters()
            .Where(n => n.Status == NotificationStatus.Read && n.CreatedAt < retentionCutoff)
            .Take(BatchSize)
            .ExecuteUpdateAsync(
                setter => setter.SetProperty(n => n.IsDeleted, true),
                ct)
            .ConfigureAwait(false);

        if (cancelledCount > 0 || purgedCount > 0)
        {
            logger.LogInformation(
                "Notification cleanup: cancelled {CancelledCount} expired, soft-deleted {PurgedCount} old read notifications",
                cancelledCount, purgedCount);
        }
    }
}

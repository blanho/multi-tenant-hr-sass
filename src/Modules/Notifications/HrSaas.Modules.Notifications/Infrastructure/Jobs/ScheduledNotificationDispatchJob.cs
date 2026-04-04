using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Infrastructure.Persistence;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Infrastructure.Jobs;

public sealed class ScheduledNotificationDispatchJob(
    NotificationsDbContext dbContext,
    IChannelProviderFactory channelProviderFactory,
    ILogger<ScheduledNotificationDispatchJob> logger) : IRecurringJob
{
    private const int BatchSize = 50;

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow;

        var scheduled = await dbContext.Notifications
            .IgnoreQueryFilters()
            .Where(n => n.Status == NotificationStatus.Pending
                && n.ScheduledAt.HasValue
                && n.ScheduledAt <= cutoff
                && !n.IsDeleted)
            .OrderBy(n => n.ScheduledAt)
            .Take(BatchSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (scheduled.Count == 0) return;

        logger.LogInformation("Dispatching {Count} scheduled notifications", scheduled.Count);

        foreach (var notification in scheduled)
        {
            try
            {
                var provider = channelProviderFactory.GetProvider(notification.Channel);
                notification.MarkAsSending();

                var result = await provider.SendAsync(new ChannelMessage(
                    notification.Id,
                    notification.TenantId,
                    notification.UserId,
                    notification.RecipientAddress ?? string.Empty,
                    notification.Subject,
                    notification.Body,
                    notification.Priority,
                    notification.Category,
                    notification.Metadata), ct).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    notification.RecordDeliveryAttempt(DeliveryStatus.Succeeded, result.ProviderResponse);
                    notification.MarkAsDelivered(result.ProviderResponse);
                }
                else
                {
                    notification.RecordDeliveryAttempt(DeliveryStatus.Failed, errorMessage: result.ErrorMessage);
                    notification.MarkAsFailed(result.ErrorMessage ?? "Delivery failed");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to dispatch scheduled notification {NotificationId}",
                    notification.Id);
                notification.MarkAsFailed(ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}

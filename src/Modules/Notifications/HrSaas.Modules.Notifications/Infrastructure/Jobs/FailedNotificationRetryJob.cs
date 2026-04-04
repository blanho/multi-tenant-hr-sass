using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Infrastructure.Persistence;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Infrastructure.Jobs;

public sealed class FailedNotificationRetryJob(
    NotificationsDbContext dbContext,
    IChannelProviderFactory channelProviderFactory,
    ILogger<FailedNotificationRetryJob> logger) : IRecurringJob
{
    private const int BatchSize = 25;

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var retryable = await dbContext.Notifications
            .IgnoreQueryFilters()
            .Where(n => n.Status == NotificationStatus.Failed
                && n.RetryCount < n.MaxRetries
                && !n.IsDeleted)
            .OrderBy(n => n.UpdatedAt)
            .Take(BatchSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (retryable.Count == 0) return;

        logger.LogInformation("Retrying {Count} failed notifications", retryable.Count);

        foreach (var notification in retryable)
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
                    notification.MarkAsFailed(result.ErrorMessage ?? "Retry failed");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Retry failed for notification {NotificationId}",
                    notification.Id);
            }
        }

        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}

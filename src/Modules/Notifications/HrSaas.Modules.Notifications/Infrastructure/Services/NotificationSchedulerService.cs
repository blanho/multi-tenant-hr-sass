using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Infrastructure.Services;

public sealed class NotificationSchedulerService(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationSchedulerService> logger) : BackgroundService
{
    private static readonly TimeSpan SchedulerInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notification scheduler service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledNotificationsAsync(stoppingToken).ConfigureAwait(false);
                await ProcessRetryableNotificationsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error in notification scheduler");
            }

            await Task.Delay(SchedulerInterval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessScheduledNotificationsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var channelFactory = scope.ServiceProvider.GetRequiredService<IChannelProviderFactory>();

        var scheduled = await repository.GetPendingScheduledAsync(
            DateTime.UtcNow, 50, ct).ConfigureAwait(false);

        if (scheduled.Count == 0) return;

        logger.LogInformation("Processing {Count} scheduled notifications", scheduled.Count);

        foreach (var notification in scheduled)
        {
            try
            {
                var provider = channelFactory.GetProvider(notification.Channel);
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

                repository.Update(notification);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process scheduled notification {NotificationId}", notification.Id);
                notification.MarkAsFailed(ex.Message);
                repository.Update(notification);
            }
        }

        await repository.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task ProcessRetryableNotificationsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var channelFactory = scope.ServiceProvider.GetRequiredService<IChannelProviderFactory>();

        var retryable = await repository.GetFailedRetryableAsync(25, ct).ConfigureAwait(false);

        if (retryable.Count == 0) return;

        logger.LogInformation("Retrying {Count} failed notifications", retryable.Count);

        foreach (var notification in retryable)
        {
            try
            {
                var provider = channelFactory.GetProvider(notification.Channel);
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

                repository.Update(notification);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Retry failed for notification {NotificationId}", notification.Id);
            }
        }

        await repository.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}

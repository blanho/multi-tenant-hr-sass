using HrSaas.Contracts.Notifications;
using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Application.Consumers;

public sealed class DispatchNotificationConsumer(
    INotificationRepository repository,
    IChannelProviderFactory channelProviderFactory,
    ILogger<DispatchNotificationConsumer> logger) : IConsumer<DispatchNotificationCommand>
{
    public async Task Consume(ConsumeContext<DispatchNotificationCommand> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "Dispatching notification {NotificationId} via {Channel} for tenant {TenantId}",
            msg.NotificationId, msg.Channel, msg.TenantId);

        var notification = await repository.GetByIdWithAttemptsAsync(
            msg.NotificationId, context.CancellationToken).ConfigureAwait(false);

        if (notification is null)
        {
            logger.LogWarning("Notification {NotificationId} not found, skipping dispatch", msg.NotificationId);
            return;
        }

        if (notification.IsExpired)
        {
            notification.Cancel();
            repository.Update(notification);
            await repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
            logger.LogInformation("Notification {NotificationId} expired, cancelled", notification.Id);
            return;
        }

        var provider = channelProviderFactory.GetProvider(notification.Channel);
        notification.MarkAsSending();

        var channelMessage = new ChannelMessage(
            notification.Id,
            notification.TenantId,
            notification.UserId,
            notification.RecipientAddress ?? string.Empty,
            notification.Subject,
            notification.Body,
            notification.Priority,
            notification.Category,
            notification.Metadata);

        var result = await provider.SendAsync(channelMessage, context.CancellationToken).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            notification.RecordDeliveryAttempt(DeliveryStatus.Succeeded, result.ProviderResponse);
            notification.MarkAsDelivered(result.ProviderResponse);
            logger.LogInformation("Notification {NotificationId} delivered via {Channel}", notification.Id, notification.Channel);
        }
        else
        {
            notification.RecordDeliveryAttempt(DeliveryStatus.Failed, errorMessage: result.ErrorMessage);
            notification.MarkAsFailed(result.ErrorMessage ?? "Unknown delivery error");
            logger.LogWarning(
                "Notification {NotificationId} delivery failed via {Channel}: {Error}",
                notification.Id, notification.Channel, result.ErrorMessage);
        }

        repository.Update(notification);
        await repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
    }
}

public sealed class SendEmailNotificationConsumer(
    INotificationRepository repository,
    IChannelProviderFactory channelProviderFactory,
    ILogger<SendEmailNotificationConsumer> logger) : IConsumer<SendEmailNotificationCommand>
{
    public async Task Consume(ConsumeContext<SendEmailNotificationCommand> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "Processing legacy email notification to {ToEmail} for tenant {TenantId}",
            msg.ToEmail, msg.TenantId);

        var provider = channelProviderFactory.GetProvider(NotificationChannel.Email);

        var channelMessage = new ChannelMessage(
            msg.Id,
            msg.TenantId,
            Guid.Empty,
            msg.ToEmail,
            msg.Subject,
            msg.BodyHtml,
            NotificationPriority.Normal,
            NotificationCategory.System,
            null);

        var result = await provider.SendAsync(channelMessage, context.CancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Legacy email notification to {ToEmail} failed: {Error}",
                msg.ToEmail, result.ErrorMessage);
        }
    }
}

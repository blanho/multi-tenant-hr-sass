using HrSaas.Contracts.Notifications;
using HrSaas.Modules.Notifications.Domain.Events;
using HrSaas.EventBus;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Application.EventHandlers;

public sealed class NotificationCreatedEventHandler(
    IEventBus eventBus,
    ILogger<NotificationCreatedEventHandler> logger) : INotificationHandler<NotificationCreatedEvent>
{
    public async Task Handle(NotificationCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Notification {NotificationId} created for user {UserId} via {Channel}, scheduling dispatch",
            notification.NotificationId, notification.UserId, notification.Channel);

        var dispatchCommand = new DispatchNotificationCommand
        {
            TenantId = notification.TenantId,
            NotificationId = notification.NotificationId,
            UserId = notification.UserId,
            Channel = notification.Channel.ToString(),
            Priority = notification.Priority.ToString()
        };

        await eventBus.PublishAsync(dispatchCommand, ct).ConfigureAwait(false);
    }
}

public sealed class NotificationDeliveredEventHandler(
    IEventBus eventBus,
    ILogger<NotificationDeliveredEventHandler> logger) : INotificationHandler<NotificationDeliveredEvent>
{
    public async Task Handle(NotificationDeliveredEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Notification {NotificationId} delivered via {Channel} at {DeliveredAt}",
            notification.NotificationId, notification.Channel, notification.DeliveredAt);

        var integrationEvent = new NotificationDeliveredIntegrationEvent
        {
            TenantId = notification.TenantId,
            NotificationId = notification.NotificationId,
            UserId = notification.UserId,
            Channel = notification.Channel.ToString(),
            DeliveredAt = notification.DeliveredAt
        };

        await eventBus.PublishAsync(integrationEvent, ct).ConfigureAwait(false);
    }
}

public sealed class NotificationFailedEventHandler(
    IEventBus eventBus,
    ILogger<NotificationFailedEventHandler> logger) : INotificationHandler<NotificationFailedEvent>
{
    public async Task Handle(NotificationFailedEvent notification, CancellationToken ct)
    {
        logger.LogWarning(
            "Notification {NotificationId} failed via {Channel} after {RetryCount} retries: {Error}",
            notification.NotificationId, notification.Channel, notification.RetryCount, notification.Error);

        var integrationEvent = new NotificationFailedIntegrationEvent
        {
            TenantId = notification.TenantId,
            NotificationId = notification.NotificationId,
            UserId = notification.UserId,
            Channel = notification.Channel.ToString(),
            Error = notification.Error,
            RetryCount = notification.RetryCount
        };

        await eventBus.PublishAsync(integrationEvent, ct).ConfigureAwait(false);
    }
}

using HrSaas.Contracts.Billing;
using HrSaas.EventBus;
using HrSaas.Modules.Billing.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Billing.Application.EventHandlers;

public sealed class SubscriptionCreatedEventHandler(
    IEventBus eventBus,
    ILogger<SubscriptionCreatedEventHandler> logger)
    : INotificationHandler<SubscriptionCreatedEvent>
{
    public async Task Handle(SubscriptionCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Subscription {SubscriptionId} created for tenant {TenantId}. Publishing integration event.",
            notification.SubscriptionId, notification.TenantId);

        await eventBus.PublishAsync(
            new SubscriptionCreatedIntegrationEvent(
                notification.TenantId,
                notification.SubscriptionId,
                notification.PlanName,
                notification.MaxSeats,
                notification.BillingCycle),
            ct).ConfigureAwait(false);
    }
}

public sealed class SubscriptionActivatedEventHandler(
    IEventBus eventBus,
    ILogger<SubscriptionActivatedEventHandler> logger)
    : INotificationHandler<SubscriptionActivatedEvent>
{
    public async Task Handle(SubscriptionActivatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Subscription {SubscriptionId} activated for tenant {TenantId}. Publishing integration event.",
            notification.SubscriptionId, notification.TenantId);

        await eventBus.PublishAsync(
            new SubscriptionActivatedIntegrationEvent(
                notification.TenantId,
                notification.SubscriptionId,
                notification.PlanName,
                notification.Price,
                notification.BillingCycle),
            ct).ConfigureAwait(false);
    }
}

public sealed class SubscriptionCancelledEventHandler(
    IEventBus eventBus,
    ILogger<SubscriptionCancelledEventHandler> logger)
    : INotificationHandler<SubscriptionCancelledEvent>
{
    public async Task Handle(SubscriptionCancelledEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Subscription {SubscriptionId} cancelled for tenant {TenantId}. Publishing integration event.",
            notification.SubscriptionId, notification.TenantId);

        await eventBus.PublishAsync(
            new SubscriptionCancelledIntegrationEvent(
                notification.TenantId,
                notification.SubscriptionId,
                notification.Reason),
            ct).ConfigureAwait(false);
    }
}

public sealed class SubscriptionPastDueEventHandler(
    IEventBus eventBus,
    ILogger<SubscriptionPastDueEventHandler> logger)
    : INotificationHandler<SubscriptionPastDueEvent>
{
    public async Task Handle(SubscriptionPastDueEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Subscription {SubscriptionId} is past due for tenant {TenantId}. Publishing integration event.",
            notification.SubscriptionId, notification.TenantId);

        await eventBus.PublishAsync(
            new SubscriptionPastDueIntegrationEvent(
                notification.TenantId,
                notification.SubscriptionId),
            ct).ConfigureAwait(false);
    }
}

using HrSaas.Contracts.Tenant;
using HrSaas.EventBus;
using HrSaas.Modules.Tenant.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Tenant.Application.EventHandlers;

public sealed class TenantCreatedEventHandler(
    IEventBus eventBus,
    ILogger<TenantCreatedEventHandler> logger)
    : INotificationHandler<TenantCreatedEvent>
{
    public async Task Handle(TenantCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Tenant {TenantId} created. Publishing integration event.", notification.TenantId);

        await eventBus.PublishAsync(
            new TenantCreatedIntegrationEvent(
                notification.TenantId,
                notification.Name,
                string.Empty,
                string.Empty,
                notification.Plan),
            ct).ConfigureAwait(false);
    }
}

public sealed class TenantSuspendedEventHandler(
    IEventBus eventBus,
    ILogger<TenantSuspendedEventHandler> logger)
    : INotificationHandler<TenantSuspendedEvent>
{
    public async Task Handle(TenantSuspendedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Tenant {TenantId} suspended. Publishing integration event.", notification.TenantId);

        await eventBus.PublishAsync(
            new TenantSuspendedIntegrationEvent(notification.TenantId, notification.Reason),
            ct).ConfigureAwait(false);
    }
}

public sealed class TenantReinstatedEventHandler(
    IEventBus eventBus,
    ILogger<TenantReinstatedEventHandler> logger)
    : INotificationHandler<TenantReinstatedEvent>
{
    public async Task Handle(TenantReinstatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Tenant {TenantId} reinstated. Publishing integration event.", notification.TenantId);

        await eventBus.PublishAsync(
            new TenantReactivatedIntegrationEvent(notification.TenantId),
            ct).ConfigureAwait(false);
    }
}

public sealed class TenantPlanUpgradedEventHandler(
    IEventBus eventBus,
    ILogger<TenantPlanUpgradedEventHandler> logger)
    : INotificationHandler<TenantPlanUpgradedEvent>
{
    public async Task Handle(TenantPlanUpgradedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Tenant {TenantId} plan upgraded from {OldPlan} to {NewPlan}. Publishing integration event.",
            notification.TenantId, notification.OldPlan, notification.NewPlan);

        await eventBus.PublishAsync(
            new TenantPlanUpgradedIntegrationEvent(
                notification.TenantId,
                notification.OldPlan,
                notification.NewPlan,
                NewMaxSeats: 0),
            ct).ConfigureAwait(false);
    }
}

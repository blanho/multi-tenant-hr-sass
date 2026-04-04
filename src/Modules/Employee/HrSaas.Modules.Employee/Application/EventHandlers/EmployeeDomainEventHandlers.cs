using HrSaas.Contracts.Employee;
using HrSaas.EventBus;
using HrSaas.Modules.Employee.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Employee.Application.EventHandlers;

public sealed class EmployeeCreatedEventHandler(
    IEventBus eventBus,
    ILogger<EmployeeCreatedEventHandler> logger)
    : INotificationHandler<EmployeeCreatedEvent>
{
    public async Task Handle(EmployeeCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Employee {EmployeeId} created for tenant {TenantId}. Publishing integration event.",
            notification.EmployeeId,
            notification.TenantId);

        var integrationEvent = new EmployeeCreatedIntegrationEvent(
            notification.TenantId,
            notification.EmployeeId,
            notification.Name,
            notification.Department,
            notification.Position,
            notification.Email);

        await eventBus.PublishAsync(integrationEvent, ct).ConfigureAwait(false);
    }
}

public sealed class EmployeeUpdatedEventHandler(
    IEventBus eventBus,
    ILogger<EmployeeUpdatedEventHandler> logger)
    : INotificationHandler<EmployeeUpdatedEvent>
{
    public async Task Handle(EmployeeUpdatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Employee {EmployeeId} updated for tenant {TenantId}. Publishing integration event.",
            notification.EmployeeId,
            notification.TenantId);

        var integrationEvent = new EmployeeUpdatedIntegrationEvent(
            notification.TenantId,
            notification.EmployeeId,
            notification.Name,
            notification.Department,
            notification.Position);

        await eventBus.PublishAsync(integrationEvent, ct).ConfigureAwait(false);
    }
}

public sealed class EmployeeDeletedEventHandler(
    IEventBus eventBus,
    ILogger<EmployeeDeletedEventHandler> logger)
    : INotificationHandler<EmployeeDeletedEvent>
{
    public async Task Handle(EmployeeDeletedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Employee {EmployeeId} deleted for tenant {TenantId}. Publishing integration event.",
            notification.EmployeeId,
            notification.TenantId);

        var integrationEvent = new EmployeeDeletedIntegrationEvent(
            notification.TenantId,
            notification.EmployeeId);

        await eventBus.PublishAsync(integrationEvent, ct).ConfigureAwait(false);
    }
}

public sealed class EmployeeDeactivatedEventHandler(
    IEventBus eventBus,
    ILogger<EmployeeDeactivatedEventHandler> logger)
    : INotificationHandler<EmployeeDeactivatedEvent>
{
    public async Task Handle(EmployeeDeactivatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Employee {EmployeeId} deactivated for tenant {TenantId}. Publishing integration event.",
            notification.EmployeeId,
            notification.TenantId);

        var integrationEvent = new EmployeeDeactivatedIntegrationEvent(
            notification.TenantId,
            notification.EmployeeId);

        await eventBus.PublishAsync(integrationEvent, ct).ConfigureAwait(false);
    }
}

public sealed class EmployeeReinstatedEventHandler(
    IEventBus eventBus,
    ILogger<EmployeeReinstatedEventHandler> logger)
    : INotificationHandler<EmployeeReinstatedEvent>
{
    public async Task Handle(EmployeeReinstatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Employee {EmployeeId} reinstated for tenant {TenantId}. Publishing integration event.",
            notification.EmployeeId,
            notification.TenantId);

        var integrationEvent = new EmployeeReinstatedIntegrationEvent(
            notification.TenantId,
            notification.EmployeeId);

        await eventBus.PublishAsync(integrationEvent, ct).ConfigureAwait(false);
    }
}

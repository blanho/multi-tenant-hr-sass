using HrSaas.EventBus;
using HrSaas.Modules.Employee.Domain.Events;
using HrSaas.Modules.Employee.Domain.IntegrationEvents;
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
            notification.Department);

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

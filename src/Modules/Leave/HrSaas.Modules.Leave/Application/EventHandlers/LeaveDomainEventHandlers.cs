using HrSaas.Contracts.Leave;
using HrSaas.EventBus;
using HrSaas.Modules.Leave.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Leave.Application.EventHandlers;

public sealed class LeaveAppliedEventHandler(
    IEventBus eventBus,
    ILogger<LeaveAppliedEventHandler> logger)
    : INotificationHandler<LeaveAppliedEvent>
{
    public async Task Handle(LeaveAppliedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Leave applied for employee {EmployeeId} in tenant {TenantId}. Publishing integration event.",
            notification.EmployeeId, notification.TenantId);

        await eventBus.PublishAsync(
            new LeaveAppliedIntegrationEvent(
                notification.TenantId,
                notification.LeaveRequestId,
                notification.EmployeeId,
                notification.LeaveType,
                StartDate: DateTime.UtcNow,
                EndDate: DateTime.UtcNow,
                DurationDays: 0,
                Reason: string.Empty),
            ct).ConfigureAwait(false);
    }
}

public sealed class LeaveApprovedEventHandler(
    IEventBus eventBus,
    ILogger<LeaveApprovedEventHandler> logger)
    : INotificationHandler<LeaveApprovedEvent>
{
    public async Task Handle(LeaveApprovedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Leave approved for employee {EmployeeId} in tenant {TenantId}. Publishing integration event.",
            notification.EmployeeId, notification.TenantId);

        await eventBus.PublishAsync(
            new LeaveApprovedIntegrationEvent(
                notification.TenantId,
                notification.LeaveRequestId,
                notification.EmployeeId,
                notification.ApprovedBy,
                LeaveType: string.Empty,
                DurationDays: 0),
            ct).ConfigureAwait(false);
    }
}

public sealed class LeaveRejectedEventHandler(
    IEventBus eventBus,
    ILogger<LeaveRejectedEventHandler> logger)
    : INotificationHandler<LeaveRejectedEvent>
{
    public async Task Handle(LeaveRejectedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Leave rejected for employee {EmployeeId} in tenant {TenantId}. Publishing integration event.",
            notification.EmployeeId, notification.TenantId);

        await eventBus.PublishAsync(
            new LeaveRejectedIntegrationEvent(
                notification.TenantId,
                notification.LeaveRequestId,
                notification.EmployeeId,
                notification.Note),
            ct).ConfigureAwait(false);
    }
}

public sealed class LeaveCancelledEventHandler(
    IEventBus eventBus,
    ILogger<LeaveCancelledEventHandler> logger)
    : INotificationHandler<LeaveCancelledEvent>
{
    public async Task Handle(LeaveCancelledEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Leave cancelled for employee {EmployeeId} in tenant {TenantId}. Publishing integration event.",
            notification.EmployeeId, notification.TenantId);

        await eventBus.PublishAsync(
            new LeaveCancelledIntegrationEvent(
                notification.TenantId,
                notification.LeaveRequestId,
                notification.EmployeeId),
            ct).ConfigureAwait(false);
    }
}

using HrSaas.Contracts.Identity;
using HrSaas.EventBus;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Identity.Application.EventHandlers;

public sealed class UserRegisteredEventHandler(
    IEventBus eventBus,
    IRoleRepository roleRepository,
    ILogger<UserRegisteredEventHandler> logger)
    : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "User {UserId} registered for tenant {TenantId}. Publishing integration event.",
            notification.UserId, notification.TenantId);

        var role = await roleRepository.GetByIdAsync(notification.RoleId, ct).ConfigureAwait(false);
        var roleName = role?.Name ?? "Unknown";

        await eventBus.PublishAsync(
            new UserRegisteredIntegrationEvent(
                notification.TenantId,
                notification.UserId,
                notification.Email,
                notification.RoleId,
                roleName),
            ct).ConfigureAwait(false);
    }
}

public sealed class UserDeactivatedEventHandler(
    IEventBus eventBus,
    ILogger<UserDeactivatedEventHandler> logger)
    : INotificationHandler<UserDeactivatedEvent>
{
    public async Task Handle(UserDeactivatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "User {UserId} deactivated for tenant {TenantId}. Publishing integration event.",
            notification.UserId, notification.TenantId);

        await eventBus.PublishAsync(
            new UserDeactivatedIntegrationEvent(
                notification.TenantId,
                notification.UserId,
                notification.Email),
            ct).ConfigureAwait(false);
    }
}

public sealed class UserRoleChangedEventHandler(
    IEventBus eventBus,
    ILogger<UserRoleChangedEventHandler> logger)
    : INotificationHandler<UserRoleChangedEvent>
{
    public async Task Handle(UserRoleChangedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "User {UserId} role changed from {OldRoleId} to {NewRoleId} in tenant {TenantId}. Publishing integration event.",
            notification.UserId, notification.OldRoleId, notification.NewRoleId, notification.TenantId);

        await eventBus.PublishAsync(
            new UserRoleChangedIntegrationEvent(
                notification.TenantId,
                notification.UserId,
                notification.OldRoleId,
                notification.NewRoleId),
            ct).ConfigureAwait(false);
    }
}

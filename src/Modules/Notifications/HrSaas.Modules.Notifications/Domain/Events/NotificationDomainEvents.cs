using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Notifications.Domain.Events;

public sealed record NotificationCreatedEvent(
    Guid TenantId,
    Guid NotificationId,
    Guid UserId,
    NotificationChannel Channel,
    NotificationCategory Category,
    NotificationPriority Priority) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record NotificationDeliveredEvent(
    Guid TenantId,
    Guid NotificationId,
    Guid UserId,
    NotificationChannel Channel,
    DateTime DeliveredAt) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record NotificationFailedEvent(
    Guid TenantId,
    Guid NotificationId,
    Guid UserId,
    NotificationChannel Channel,
    string Error,
    int RetryCount) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record NotificationReadEvent(
    Guid TenantId,
    Guid NotificationId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

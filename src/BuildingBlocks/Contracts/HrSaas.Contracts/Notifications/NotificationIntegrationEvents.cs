using HrSaas.SharedKernel.Events;

namespace HrSaas.Contracts.Notifications;

public sealed record DispatchNotificationCommand : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
    public required string Channel { get; init; }
    public required string Priority { get; init; }
}

public sealed record NotificationDeliveredIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
    public required string Channel { get; init; }
    public DateTime DeliveredAt { get; init; }
}

public sealed record NotificationFailedIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
    public required string Channel { get; init; }
    public required string Error { get; init; }
    public int RetryCount { get; init; }
}

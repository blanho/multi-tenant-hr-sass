using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Events;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Notifications.Domain.Entities;

public sealed class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationCategory Category { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public Guid? TemplateId { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? Metadata { get; private set; }
    public string? RecipientAddress { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public string? LastError { get; private set; }
    public Guid? GroupId { get; private set; }

    private readonly List<DeliveryAttempt> _deliveryAttempts = [];
    public IReadOnlyList<DeliveryAttempt> DeliveryAttempts => _deliveryAttempts.AsReadOnly();

    public bool CanRetry => RetryCount < MaxRetries
                            && Status == NotificationStatus.Failed
                            && ExpiresAt is null || ExpiresAt > DateTime.UtcNow;

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    private Notification() { }

    public static Notification Create(
        Guid tenantId,
        Guid userId,
        NotificationChannel channel,
        NotificationCategory category,
        NotificationPriority priority,
        string subject,
        string body,
        string? recipientAddress = null,
        Guid? templateId = null,
        string? correlationId = null,
        string? metadata = null,
        DateTime? scheduledAt = null,
        DateTime? expiresAt = null,
        int maxRetries = 3,
        Guid? groupId = null)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        Guard.NotEmpty(userId, nameof(userId));
        Guard.NotNullOrWhiteSpace(subject, nameof(subject));
        Guard.NotNullOrWhiteSpace(body, nameof(body));

        var notification = new Notification
        {
            TenantId = tenantId,
            UserId = userId,
            Channel = channel,
            Category = category,
            Priority = priority,
            Subject = subject,
            Body = body,
            RecipientAddress = recipientAddress,
            Status = scheduledAt.HasValue && scheduledAt > DateTime.UtcNow
                ? NotificationStatus.Pending
                : NotificationStatus.Queued,
            TemplateId = templateId,
            CorrelationId = correlationId,
            Metadata = metadata,
            ScheduledAt = scheduledAt,
            ExpiresAt = expiresAt,
            MaxRetries = maxRetries,
            GroupId = groupId
        };

        notification.AddDomainEvent(new NotificationCreatedEvent(
            tenantId, notification.Id, userId, channel, category, priority));

        return notification;
    }

    public void MarkAsSending()
    {
        Status = NotificationStatus.Sending;
        Touch();
    }

    public void MarkAsDelivered(string? providerResponse = null)
    {
        Status = NotificationStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        Touch();

        AddDomainEvent(new NotificationDeliveredEvent(
            TenantId, Id, UserId, Channel, DeliveredAt.Value));
    }

    public void MarkAsFailed(string error)
    {
        Guard.NotNullOrWhiteSpace(error, nameof(error));

        RetryCount++;
        LastError = error;
        Status = CanRetry ? NotificationStatus.Queued : NotificationStatus.Failed;
        Touch();

        if (Status == NotificationStatus.Failed)
        {
            AddDomainEvent(new NotificationFailedEvent(
                TenantId, Id, UserId, Channel, error, RetryCount));
        }
    }

    public void MarkAsRead()
    {
        if (ReadAt.HasValue) return;

        ReadAt = DateTime.UtcNow;
        if (Status == NotificationStatus.Delivered)
            Status = NotificationStatus.Read;
        Touch();

        AddDomainEvent(new NotificationReadEvent(TenantId, Id, UserId));
    }

    public void Cancel()
    {
        if (Status is NotificationStatus.Delivered or NotificationStatus.Read) return;

        Status = NotificationStatus.Cancelled;
        Touch();
    }

    public DeliveryAttempt RecordDeliveryAttempt(
        DeliveryStatus status,
        string? providerResponse = null,
        string? errorMessage = null)
    {
        var attempt = DeliveryAttempt.Create(
            Id,
            _deliveryAttempts.Count + 1,
            status,
            providerResponse,
            errorMessage);

        _deliveryAttempts.Add(attempt);
        return attempt;
    }
}

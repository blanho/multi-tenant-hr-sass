using HrSaas.Modules.Notifications.Domain.Enums;

namespace HrSaas.Modules.Notifications.Domain.Entities;

public sealed class DeliveryAttempt
{
    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }
    public int AttemptNumber { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public string? ProviderResponse { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime AttemptedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long? DurationMs { get; private set; }

    private DeliveryAttempt() { }

    public static DeliveryAttempt Create(
        Guid notificationId,
        int attemptNumber,
        DeliveryStatus status,
        string? providerResponse = null,
        string? errorMessage = null)
    {
        var now = DateTime.UtcNow;

        return new DeliveryAttempt
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            AttemptNumber = attemptNumber,
            Status = status,
            ProviderResponse = providerResponse,
            ErrorMessage = errorMessage,
            AttemptedAt = now,
            CompletedAt = now,
            DurationMs = 0
        };
    }

    public void Complete(DeliveryStatus status, string? providerResponse = null, string? errorMessage = null)
    {
        Status = status;
        ProviderResponse = providerResponse;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        DurationMs = (long)(CompletedAt.Value - AttemptedAt).TotalMilliseconds;
    }
}

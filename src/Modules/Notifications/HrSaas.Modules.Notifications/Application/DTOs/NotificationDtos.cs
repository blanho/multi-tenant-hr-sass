using HrSaas.Modules.Notifications.Domain.Enums;

namespace HrSaas.Modules.Notifications.Application.DTOs;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    NotificationChannel Channel,
    NotificationCategory Category,
    NotificationPriority Priority,
    string Subject,
    string Body,
    NotificationStatus Status,
    DateTime CreatedAt,
    DateTime? ReadAt,
    DateTime? DeliveredAt,
    string? CorrelationId);

public sealed record NotificationDetailDto(
    Guid Id,
    Guid UserId,
    NotificationChannel Channel,
    NotificationCategory Category,
    NotificationPriority Priority,
    string Subject,
    string Body,
    NotificationStatus Status,
    DateTime CreatedAt,
    DateTime? ReadAt,
    DateTime? DeliveredAt,
    DateTime? ScheduledAt,
    DateTime? ExpiresAt,
    string? CorrelationId,
    string? Metadata,
    string? RecipientAddress,
    int RetryCount,
    int MaxRetries,
    string? LastError,
    IReadOnlyList<DeliveryAttemptDto> DeliveryAttempts);

public sealed record DeliveryAttemptDto(
    Guid Id,
    int AttemptNumber,
    DeliveryStatus Status,
    string? ProviderResponse,
    string? ErrorMessage,
    DateTime AttemptedAt,
    DateTime? CompletedAt,
    long? DurationMs);

public sealed record UserPreferenceDto(
    Guid Id,
    Guid UserId,
    NotificationChannel Channel,
    NotificationCategory Category,
    bool IsEnabled,
    DigestFrequency DigestFrequency,
    TimeOnly? QuietHoursStart,
    TimeOnly? QuietHoursEnd,
    string? TimeZone);

public sealed record NotificationTemplateDto(
    Guid Id,
    string Name,
    string Slug,
    NotificationChannel Channel,
    NotificationCategory Category,
    string SubjectTemplate,
    string BodyTemplate,
    bool IsActive,
    string? Description);

public sealed record NotificationPagedResult(
    IReadOnlyList<NotificationDto> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}

public sealed record NotificationStatsDto(
    int TotalCount,
    int UnreadCount,
    int DeliveredCount,
    int FailedCount,
    Dictionary<NotificationCategory, int> ByCategory);

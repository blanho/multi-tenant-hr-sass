using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Notifications.Domain.Entities;

public sealed class UserNotificationPreference : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationCategory Category { get; private set; }
    public bool IsEnabled { get; private set; }
    public DigestFrequency DigestFrequency { get; private set; }
    public TimeOnly? QuietHoursStart { get; private set; }
    public TimeOnly? QuietHoursEnd { get; private set; }
    public string? TimeZone { get; private set; }

    private UserNotificationPreference() { }

    public static UserNotificationPreference Create(
        Guid tenantId,
        Guid userId,
        NotificationChannel channel,
        NotificationCategory category,
        bool isEnabled = true,
        DigestFrequency digestFrequency = DigestFrequency.Immediate,
        TimeOnly? quietHoursStart = null,
        TimeOnly? quietHoursEnd = null,
        string? timeZone = null)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        Guard.NotEmpty(userId, nameof(userId));

        return new UserNotificationPreference
        {
            TenantId = tenantId,
            UserId = userId,
            Channel = channel,
            Category = category,
            IsEnabled = isEnabled,
            DigestFrequency = digestFrequency,
            QuietHoursStart = quietHoursStart,
            QuietHoursEnd = quietHoursEnd,
            TimeZone = timeZone ?? "UTC"
        };
    }

    public void UpdatePreference(
        bool isEnabled,
        DigestFrequency digestFrequency,
        TimeOnly? quietHoursStart = null,
        TimeOnly? quietHoursEnd = null,
        string? timeZone = null)
    {
        IsEnabled = isEnabled;
        DigestFrequency = digestFrequency;
        QuietHoursStart = quietHoursStart;
        QuietHoursEnd = quietHoursEnd;
        TimeZone = timeZone ?? TimeZone;
        Touch();
    }

    public bool IsInQuietHours(DateTime utcNow)
    {
        if (!QuietHoursStart.HasValue || !QuietHoursEnd.HasValue) return false;

        var tz = TimeZoneInfo.FindSystemTimeZoneById(TimeZone ?? "UTC");
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
        var currentTime = TimeOnly.FromDateTime(localTime);

        if (QuietHoursStart.Value <= QuietHoursEnd.Value)
            return currentTime >= QuietHoursStart.Value && currentTime <= QuietHoursEnd.Value;

        return currentTime >= QuietHoursStart.Value || currentTime <= QuietHoursEnd.Value;
    }
}

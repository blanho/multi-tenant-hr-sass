namespace HrSaas.Modules.Notifications.Domain.Enums;

public enum NotificationChannel
{
    InApp = 0,
    Email = 1,
    Sms = 2,
    Push = 3,
    Slack = 4,
    Webhook = 5
}

public enum NotificationStatus
{
    Pending = 0,
    Queued = 1,
    Sending = 2,
    Delivered = 3,
    Failed = 4,
    Read = 5,
    Cancelled = 6
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public enum NotificationCategory
{
    System = 0,
    Employee = 1,
    Leave = 2,
    Billing = 3,
    Security = 4,
    Onboarding = 5,
    Performance = 6,
    Announcement = 7
}

public enum DeliveryStatus
{
    Attempted = 0,
    Succeeded = 1,
    Failed = 2,
    Bounced = 3,
    Throttled = 4
}

public enum DigestFrequency
{
    Immediate = 0,
    Hourly = 1,
    Daily = 2,
    Weekly = 3,
    Never = 4
}

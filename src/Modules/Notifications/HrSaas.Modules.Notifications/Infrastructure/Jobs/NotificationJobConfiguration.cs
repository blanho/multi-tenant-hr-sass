using HrSaas.SharedKernel.Jobs;

namespace HrSaas.Modules.Notifications.Infrastructure.Jobs;

public sealed class NotificationJobConfiguration : IRecurringJobConfiguration
{
    public IReadOnlyList<RecurringJobDefinition> GetRecurringJobs() =>
    [
        new RecurringJobDefinition(
            "notifications:dispatch-scheduled",
            typeof(ScheduledNotificationDispatchJob),
            "* * * * *",
            "notifications"),

        new RecurringJobDefinition(
            "notifications:retry-failed",
            typeof(FailedNotificationRetryJob),
            "*/5 * * * *",
            "notifications"),

        new RecurringJobDefinition(
            "notifications:daily-digest",
            typeof(NotificationDigestJob),
            "0 8 * * *",
            "notifications"),

        new RecurringJobDefinition(
            "notifications:cleanup-expired",
            typeof(ExpiredNotificationCleanupJob),
            "0 2 * * *",
            "maintenance")
    ];
}

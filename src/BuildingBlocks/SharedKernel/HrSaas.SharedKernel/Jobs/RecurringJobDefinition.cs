namespace HrSaas.SharedKernel.Jobs;

public sealed record RecurringJobDefinition(
    string JobId,
    Type JobType,
    string CronExpression,
    string Queue = "default",
    TimeZoneInfo? TimeZone = null);

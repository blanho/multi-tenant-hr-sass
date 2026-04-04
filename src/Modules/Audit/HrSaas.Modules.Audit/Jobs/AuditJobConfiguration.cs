using HrSaas.SharedKernel.Jobs;

namespace HrSaas.Modules.Audit.Jobs;

public sealed class AuditJobConfiguration : IRecurringJobConfiguration
{
    public IReadOnlyList<RecurringJobDefinition> GetRecurringJobs() =>
    [
        new RecurringJobDefinition(
            JobId: "audit:log-retention",
            JobType: typeof(AuditLogRetentionJob),
            CronExpression: "0 4 * * *",
            Queue: "maintenance")
    ];
}

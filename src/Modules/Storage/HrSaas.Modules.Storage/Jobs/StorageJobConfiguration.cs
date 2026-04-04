using HrSaas.SharedKernel.Jobs;

namespace HrSaas.Modules.Storage.Jobs;

public sealed class StorageJobConfiguration : IRecurringJobConfiguration
{
    public IReadOnlyList<RecurringJobDefinition> GetRecurringJobs() =>
    [
        new RecurringJobDefinition(
            JobId: "orphaned-file-cleanup",
            JobType: typeof(OrphanedFileCleanupJob),
            CronExpression: "0 3 * * *",
            Queue: "maintenance")
    ];
}

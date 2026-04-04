using HrSaas.SharedKernel.Jobs;

namespace HrSaas.EventBus.Outbox;

public sealed class OutboxJobConfiguration : IRecurringJobConfiguration
{
    public IReadOnlyList<RecurringJobDefinition> GetRecurringJobs() =>
    [
        new RecurringJobDefinition(
            "outbox:process",
            typeof(OutboxProcessorJob),
            "* * * * *",
            "critical"),

        new RecurringJobDefinition(
            "outbox:cleanup",
            typeof(OutboxCleanupJob),
            "0 3 * * *",
            "maintenance")
    ];
}

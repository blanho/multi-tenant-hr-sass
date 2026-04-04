using HrSaas.SharedKernel.Jobs;

namespace HrSaas.Modules.Leave.Infrastructure.Jobs;

public sealed class LeaveJobConfiguration : IRecurringJobConfiguration
{
    public IReadOnlyList<RecurringJobDefinition> GetRecurringJobs() =>
    [
        new RecurringJobDefinition(
            "leave:annual-accrual",
            typeof(LeaveAccrualResetJob),
            "0 0 1 1 *")
    ];
}

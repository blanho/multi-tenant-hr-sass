namespace HrSaas.SharedKernel.Jobs;

public interface IRecurringJobConfiguration
{
    IReadOnlyList<RecurringJobDefinition> GetRecurringJobs();
}

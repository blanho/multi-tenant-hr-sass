namespace HrSaas.SharedKernel.Jobs;

public interface IJobScheduler
{
    string Enqueue<TJob>() where TJob : class, IRecurringJob;
    string Schedule<TJob>(TimeSpan delay) where TJob : class, IRecurringJob;
    void TriggerRecurring(string jobId);
    void RemoveRecurring(string jobId);
}

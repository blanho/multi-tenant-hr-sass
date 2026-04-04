using Hangfire;
using HrSaas.SharedKernel.Jobs;

namespace HrSaas.JobScheduler;

public sealed class HangfireJobScheduler(
    IBackgroundJobClient jobClient,
    IRecurringJobManager recurringJobManager) : IJobScheduler
{
    public string Enqueue<TJob>() where TJob : class, IRecurringJob
        => jobClient.Enqueue<TJob>(job => job.ExecuteAsync(CancellationToken.None));

    public string Schedule<TJob>(TimeSpan delay) where TJob : class, IRecurringJob
        => jobClient.Schedule<TJob>(job => job.ExecuteAsync(CancellationToken.None), delay);

    public void TriggerRecurring(string jobId)
        => RecurringJob.TriggerJob(jobId);

    public void RemoveRecurring(string jobId)
        => recurringJobManager.RemoveIfExists(jobId);
}

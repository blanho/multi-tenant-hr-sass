namespace HrSaas.SharedKernel.Jobs;

public interface IRecurringJob
{
    Task ExecuteAsync(CancellationToken ct = default);
}

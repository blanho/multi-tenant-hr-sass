using Microsoft.Extensions.Http.Resilience;

namespace HrSaas.Api.Infrastructure.Resilience;

public static class ResilienceExtensions
{
    public static IHttpClientBuilder AddStandardResilience(this IHttpClientBuilder builder)
    {
        builder.AddStandardResilienceHandler(opts =>
        {
            opts.Retry.MaxRetryAttempts = 3;
            opts.Retry.Delay = TimeSpan.FromMilliseconds(200);
            opts.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            opts.CircuitBreaker.FailureRatio = 0.5;
            opts.CircuitBreaker.MinimumThroughput = 10;
            opts.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });
        return builder;
    }
}

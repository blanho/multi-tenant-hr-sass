using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace HrSaas.Api.Infrastructure.Observability;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["Telemetry:ServiceName"] ?? "HrSaas.Api";
        var otlpEndpoint = configuration["Telemetry:OtlpEndpoint"] ?? "http://localhost:4317";
        var appInsightsConnectionString = configuration["Azure:ApplicationInsights:ConnectionString"];

        var otelBuilder = services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName));

        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            otelBuilder.UseAzureMonitor(opts =>
                opts.ConnectionString = appInsightsConnectionString);
        }

        otelBuilder
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource(HrSaasActivitySource.SourceName);

                if (string.IsNullOrWhiteSpace(appInsightsConnectionString))
                    tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (string.IsNullOrWhiteSpace(appInsightsConnectionString))
                    metrics.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
            });

        return services;
    }
}

public static class HrSaasActivitySource
{
    public const string SourceName = "HrSaas";
    private static readonly System.Diagnostics.ActivitySource Source = new(SourceName);

    public static System.Diagnostics.Activity? StartActivity(string name, System.Diagnostics.ActivityKind kind = System.Diagnostics.ActivityKind.Internal) =>
        Source.StartActivity(name, kind);
}

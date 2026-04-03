using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HrSaas.Api.Infrastructure.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddModuleHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var rabbitMq = configuration.GetConnectionString("RabbitMQ")!;
        var redis = configuration.GetConnectionString("Redis")!;

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: ["db", "readiness"])
            .AddRabbitMQ(rabbitMq, name: "rabbitmq", tags: ["messaging", "readiness"])
            .AddRedis(redis, name: "redis", tags: ["cache", "readiness"]);

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = (ctx, _) =>
            {
                ctx.Response.ContentType = "text/plain";
                return ctx.Response.WriteAsync("healthy");
            }
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Contains("readiness"),
            ResponseWriter = HealthCheckUiResponseWriter
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckUiResponseWriter
        });

        return app;
    }

    private static readonly Func<HttpContext, HealthReport, Task> HealthCheckUiResponseWriter =
        HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse;
}

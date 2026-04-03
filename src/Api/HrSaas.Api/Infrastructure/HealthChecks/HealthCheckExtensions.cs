using Azure.Identity;
using HealthChecks.UI.Client;
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

        var builder = services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: ["db", "readiness"])
            .AddRedis(redis, name: "redis", tags: ["cache", "readiness"]);

        var azureServiceBus = configuration.GetConnectionString("AzureServiceBus");
        if (!string.IsNullOrWhiteSpace(azureServiceBus))
        {
            builder.AddAzureServiceBusTopic(
                azureServiceBus,
                topicName: "hr-saas-health",
                name: "azure-servicebus",
                tags: ["messaging", "readiness", "azure"]);
        }
        else
        {
            builder.AddRabbitMQ(rabbitMq, name: "rabbitmq", tags: ["messaging", "readiness"]);
        }

        var blobConnectionString = configuration["Azure:BlobStorage:ConnectionString"];
        var blobServiceUri = configuration["Azure:BlobStorage:ServiceUri"];
        if (!string.IsNullOrWhiteSpace(blobConnectionString))
        {
            builder.AddAzureBlobStorage(
                sp => new global::Azure.Storage.Blobs.BlobServiceClient(blobConnectionString),
                name: "azure-blob",
                tags: ["storage", "readiness", "azure"]);
        }
        else if (!string.IsNullOrWhiteSpace(blobServiceUri))
        {
            builder.AddAzureBlobStorage(
                sp => new global::Azure.Storage.Blobs.BlobServiceClient(
                    new Uri(blobServiceUri), new DefaultAzureCredential()),
                name: "azure-blob",
                tags: ["storage", "readiness", "azure"]);
        }

        var keyVaultUri = configuration["Azure:KeyVault:VaultUri"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential(),
                setup => { },
                name: "azure-keyvault",
                tags: ["secrets", "readiness", "azure"]);
        }

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
        UIResponseWriter.WriteHealthCheckUIResponse;
}

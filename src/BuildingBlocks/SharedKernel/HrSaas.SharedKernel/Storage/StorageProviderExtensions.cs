using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.SharedKernel.Storage;

public static class StorageProviderExtensions
{
    public static IServiceCollection AddStorageProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StorageProviderOptions>(configuration.GetSection(StorageProviderOptions.SectionName));

        var provider = configuration[$"{StorageProviderOptions.SectionName}:Provider"] ?? "Local";

        if (provider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            RegisterAzureBlobClient(services, configuration);
            services.AddScoped<IStorageProvider, AzureBlobStorageProvider>();
        }
        else
        {
            services.AddScoped<IStorageProvider, LocalFileStorageProvider>();
        }

        return services;
    }

    private static void RegisterAzureBlobClient(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration[$"{StorageProviderOptions.SectionName}:AzureConnectionString"]
                            ?? configuration["Azure:BlobStorage:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton(new BlobServiceClient(connectionString));
            return;
        }

        var serviceUri = configuration[$"{StorageProviderOptions.SectionName}:AzureServiceUri"]
                      ?? configuration["Azure:BlobStorage:ServiceUri"];

        if (!string.IsNullOrWhiteSpace(serviceUri))
        {
            services.AddSingleton(_ =>
                new BlobServiceClient(
                    new Uri(serviceUri),
                    new Azure.Identity.DefaultAzureCredential()));
        }
    }
}

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HrSaas.SharedKernel.Storage;

public interface IBlobStorageService
{
    Task<string> UploadAsync(
        Guid tenantId,
        string containerSuffix,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken ct = default);

    Task<Stream?> DownloadAsync(
        Guid tenantId,
        string containerSuffix,
        string fileName,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(
        Guid tenantId,
        string containerSuffix,
        string fileName,
        CancellationToken ct = default);

    Task<IReadOnlyList<string>> ListAsync(
        Guid tenantId,
        string containerSuffix,
        string? prefix = null,
        CancellationToken ct = default);
}

public sealed class AzureBlobStorageService(
    BlobServiceClient blobServiceClient,
    ILogger<AzureBlobStorageService> logger) : IBlobStorageService
{
    public async Task<string> UploadAsync(
        Guid tenantId,
        string containerSuffix,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken ct = default)
    {
        var containerClient = await GetContainerAsync(tenantId, containerSuffix, ct).ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(
            content,
            new BlobHttpHeaders { ContentType = contentType },
            metadata: new Dictionary<string, string> { ["tenantId"] = tenantId.ToString() },
            cancellationToken: ct).ConfigureAwait(false);

        logger.LogInformation(
            "Uploaded blob {FileName} to container {Container} for tenant {TenantId}",
            fileName, containerClient.Name, tenantId);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream?> DownloadAsync(
        Guid tenantId,
        string containerSuffix,
        string fileName,
        CancellationToken ct = default)
    {
        var containerClient = await GetContainerAsync(tenantId, containerSuffix, ct).ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync(ct).ConfigureAwait(false))
            return null;

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct).ConfigureAwait(false);
        return response.Value.Content;
    }

    public async Task<bool> DeleteAsync(
        Guid tenantId,
        string containerSuffix,
        string fileName,
        CancellationToken ct = default)
    {
        var containerClient = await GetContainerAsync(tenantId, containerSuffix, ct).ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(fileName);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: ct).ConfigureAwait(false);

        logger.LogInformation(
            "Deleted blob {FileName} from container {Container} for tenant {TenantId} (existed: {Existed})",
            fileName, containerClient.Name, tenantId, response.Value);

        return response.Value;
    }

    public async Task<IReadOnlyList<string>> ListAsync(
        Guid tenantId,
        string containerSuffix,
        string? prefix = null,
        CancellationToken ct = default)
    {
        var containerClient = await GetContainerAsync(tenantId, containerSuffix, ct).ConfigureAwait(false);
        var blobs = new List<string>();

        await foreach (var blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: ct).ConfigureAwait(false))
        {
            blobs.Add(blob.Name);
        }

        return blobs.AsReadOnly();
    }

    private async Task<BlobContainerClient> GetContainerAsync(
        Guid tenantId, string suffix, CancellationToken ct)
    {
        var containerName = $"{tenantId.ToString()[..8]}-{suffix}".ToLowerInvariant();
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None, cancellationToken: ct).ConfigureAwait(false);
        return containerClient;
    }
}

public static class BlobStorageExtensions
{
    public static IServiceCollection AddAzureBlobStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["Azure:BlobStorage:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton(new BlobServiceClient(connectionString));
        }
        else
        {
            var serviceUri = configuration["Azure:BlobStorage:ServiceUri"];
            if (!string.IsNullOrWhiteSpace(serviceUri))
            {
                services.AddSingleton(sp =>
                    new BlobServiceClient(
                        new Uri(serviceUri),
                        new Azure.Identity.DefaultAzureCredential()));
            }
        }

        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        return services;
    }
}

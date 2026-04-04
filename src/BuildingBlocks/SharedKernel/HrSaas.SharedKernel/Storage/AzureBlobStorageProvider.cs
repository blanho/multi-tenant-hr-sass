using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

namespace HrSaas.SharedKernel.Storage;

public sealed class AzureBlobStorageProvider(
    BlobServiceClient blobServiceClient,
    ILogger<AzureBlobStorageProvider> logger) : IStorageProvider
{
    public async Task<StorageUploadResult> UploadAsync(
        Guid tenantId,
        string blobName,
        Stream content,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
        var containerClient = await GetContainerAsync(tenantId, ct).ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        var blobMetadata = new Dictionary<string, string> { ["tenantId"] = tenantId.ToString() };

        if (metadata is not null)
        {
            foreach (var kvp in metadata)
                blobMetadata[kvp.Key] = kvp.Value;
        }

        await blobClient.UploadAsync(content, headers, blobMetadata, cancellationToken: ct)
            .ConfigureAwait(false);

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: ct).ConfigureAwait(false);

        logger.LogInformation(
            "Uploaded blob {BlobName} to container {Container} for tenant {TenantId} ({Size} bytes)",
            blobName, containerClient.Name, tenantId, properties.Value.ContentLength);

        return new StorageUploadResult(
            blobName,
            containerClient.Name,
            blobClient.Uri.ToString(),
            properties.Value.ContentLength);
    }

    public async Task<Stream?> DownloadAsync(
        Guid tenantId,
        string blobName,
        CancellationToken ct = default)
    {
        var containerClient = await GetContainerAsync(tenantId, ct).ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(ct).ConfigureAwait(false))
            return null;

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct).ConfigureAwait(false);
        return response.Value.Content;
    }

    public async Task<bool> DeleteAsync(
        Guid tenantId,
        string blobName,
        CancellationToken ct = default)
    {
        var containerClient = await GetContainerAsync(tenantId, ct).ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: ct).ConfigureAwait(false);

        logger.LogInformation(
            "Deleted blob {BlobName} from container {Container} for tenant {TenantId} (existed: {Existed})",
            blobName, containerClient.Name, tenantId, response.Value);

        return response.Value;
    }

    public async Task<bool> ExistsAsync(
        Guid tenantId,
        string blobName,
        CancellationToken ct = default)
    {
        var containerClient = await GetContainerAsync(tenantId, ct).ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);
        return await blobClient.ExistsAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> GetPresignedUrlAsync(
        Guid tenantId,
        string blobName,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        var containerClient = await GetContainerAsync(tenantId, ct).ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        var delegationKey = await blobServiceClient
            .GetUserDelegationKeyAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.Add(expiry), ct)
            .ConfigureAwait(false);

        var sasBuilderDelegation = new BlobSasBuilder
        {
            BlobContainerName = containerClient.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilderDelegation.SetPermissions(BlobSasPermissions.Read);

        var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
        {
            Sas = sasBuilderDelegation.ToSasQueryParameters(delegationKey.Value, blobServiceClient.AccountName)
        };

        return blobUriBuilder.ToUri().ToString();
    }

    private async Task<BlobContainerClient> GetContainerAsync(Guid tenantId, CancellationToken ct)
    {
        var containerName = $"{tenantId.ToString()[..8]}-files".ToLowerInvariant();
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct)
            .ConfigureAwait(false);
        return containerClient;
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrSaas.SharedKernel.Storage;

public sealed class LocalFileStorageProvider(
    IOptions<StorageProviderOptions> options,
    ILogger<LocalFileStorageProvider> logger) : IStorageProvider
{
    private readonly string _basePath = options.Value.LocalBasePath;
    private readonly string _baseUrl = options.Value.LocalBaseUrl;

    public async Task<StorageUploadResult> UploadAsync(
        Guid tenantId,
        string blobName,
        Stream content,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
        var directory = GetTenantDirectory(tenantId);
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, blobName);
        var fileDirectory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(fileDirectory);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, ct).ConfigureAwait(false);
        var length = fileStream.Length;

        logger.LogInformation(
            "Stored file {BlobName} locally for tenant {TenantId} at {FilePath} ({Size} bytes)",
            blobName, tenantId, filePath, length);

        return new StorageUploadResult(
            blobName,
            $"tenant-{tenantId.ToString()[..8]}",
            filePath,
            length);
    }

    public async Task<Stream?> DownloadAsync(
        Guid tenantId,
        string blobName,
        CancellationToken ct = default)
    {
        var filePath = GetFilePath(tenantId, blobName);

        if (!File.Exists(filePath))
            return null;

        var memoryStream = new MemoryStream();
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        await fileStream.CopyToAsync(memoryStream, ct).ConfigureAwait(false);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public Task<bool> DeleteAsync(
        Guid tenantId,
        string blobName,
        CancellationToken ct = default)
    {
        var filePath = GetFilePath(tenantId, blobName);

        if (!File.Exists(filePath))
            return Task.FromResult(false);

        File.Delete(filePath);

        logger.LogInformation(
            "Deleted local file {BlobName} for tenant {TenantId}",
            blobName, tenantId);

        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(
        Guid tenantId,
        string blobName,
        CancellationToken ct = default)
    {
        var filePath = GetFilePath(tenantId, blobName);
        return Task.FromResult(File.Exists(filePath));
    }

    public Task<string> GetPresignedUrlAsync(
        Guid tenantId,
        string blobName,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/{blobName}/download";
        return Task.FromResult(url);
    }

    private string GetTenantDirectory(Guid tenantId)
        => Path.Combine(_basePath, $"tenant-{tenantId.ToString()[..8]}");

    private string GetFilePath(Guid tenantId, string blobName)
        => Path.Combine(GetTenantDirectory(tenantId), blobName);
}

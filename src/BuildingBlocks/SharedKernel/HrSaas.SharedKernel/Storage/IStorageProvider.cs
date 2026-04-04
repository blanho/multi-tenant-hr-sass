namespace HrSaas.SharedKernel.Storage;

public interface IStorageProvider
{
    Task<StorageUploadResult> UploadAsync(
        Guid tenantId,
        string blobName,
        Stream content,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken ct = default);

    Task<Stream?> DownloadAsync(
        Guid tenantId,
        string blobName,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(
        Guid tenantId,
        string blobName,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        Guid tenantId,
        string blobName,
        CancellationToken ct = default);

    Task<string> GetPresignedUrlAsync(
        Guid tenantId,
        string blobName,
        TimeSpan expiry,
        CancellationToken ct = default);
}

public sealed record StorageUploadResult(
    string BlobName,
    string ContainerName,
    string FullPath,
    long ContentLength);

public sealed class StorageProviderOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";
    public string AzureConnectionString { get; set; } = string.Empty;
    public string AzureServiceUri { get; set; } = string.Empty;
    public string LocalBasePath { get; set; } = "./storage-data";
    public string LocalBaseUrl { get; set; } = "http://localhost:5000/api/v1/files";
    public long MaxFileSizeBytes { get; set; } = 52_428_800;
    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/svg+xml",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain", "text/csv",
        "application/zip", "application/x-7z-compressed",
        "application/json", "application/xml"
    ];
}

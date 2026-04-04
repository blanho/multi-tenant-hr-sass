using HrSaas.Modules.Storage.Domain.Enums;
using HrSaas.SharedKernel.Entities;

namespace HrSaas.Modules.Storage.Domain.Entities;

public sealed class StoredFile : BaseEntity
{
    public string OriginalFileName { get; private set; } = string.Empty;
    public string BlobName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public FileCategory Category { get; private set; }
    public FileStatus Status { get; private set; }
    public string? EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public Guid UploadedBy { get; private set; }
    public string? Description { get; private set; }
    public string? Checksum { get; private set; }

    private StoredFile() { }

    public static StoredFile Create(
        Guid tenantId,
        string originalFileName,
        string blobName,
        string contentType,
        long sizeBytes,
        FileCategory category,
        Guid uploadedBy,
        string? entityType = null,
        string? entityId = null,
        string? description = null,
        string? checksum = null)
    {
        return new StoredFile
        {
            TenantId = tenantId,
            OriginalFileName = originalFileName,
            BlobName = blobName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Category = category,
            Status = FileStatus.Active,
            EntityType = entityType,
            EntityId = entityId,
            UploadedBy = uploadedBy,
            Description = description,
            Checksum = checksum
        };
    }

    public void Archive()
    {
        Status = FileStatus.Archived;
        Touch();
    }

    public void MarkForDeletion()
    {
        Status = FileStatus.PendingDeletion;
        Touch();
    }

    public void MarkOrphaned()
    {
        Status = FileStatus.Orphaned;
        Touch();
    }

    public void AttachToEntity(string entityType, string entityId)
    {
        EntityType = entityType;
        EntityId = entityId;
        Touch();
    }
}

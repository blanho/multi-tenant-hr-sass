using HrSaas.Modules.Storage.Domain.Enums;

namespace HrSaas.Modules.Storage.Application.DTOs;

public sealed record StoredFileDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    FileCategory Category,
    FileStatus Status,
    string? EntityType,
    string? EntityId,
    Guid UploadedBy,
    string? Description,
    DateTime CreatedAt);

public sealed record StoredFileDetailDto(
    Guid Id,
    string OriginalFileName,
    string BlobName,
    string ContentType,
    long SizeBytes,
    FileCategory Category,
    FileStatus Status,
    string? EntityType,
    string? EntityId,
    Guid UploadedBy,
    string? Description,
    string? Checksum,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record FileUploadResultDto(
    Guid FileId,
    string OriginalFileName,
    long SizeBytes,
    string ContentType);

public sealed record PresignedUrlDto(
    string Url,
    DateTime ExpiresAt);

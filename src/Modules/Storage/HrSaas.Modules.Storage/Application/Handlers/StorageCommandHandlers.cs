using System.Security.Cryptography;
using HrSaas.Modules.Storage.Application.Commands;
using HrSaas.Modules.Storage.Domain.Entities;
using HrSaas.Modules.Storage.Domain.Repositories;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.CQRS;
using HrSaas.SharedKernel.Storage;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Storage.Application.Handlers;

[Auditable(AuditAction.Upload, AuditCategory.Storage, Severity = AuditSeverity.Medium)]
public sealed class UploadFileCommandHandler(
    IStorageProvider storageProvider,
    IStoredFileRepository repository,
    ILogger<UploadFileCommandHandler> logger) : IRequestHandler<UploadFileCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var blobName = GenerateBlobName(request.OriginalFileName);

        string? checksum;
        using (var sha256 = SHA256.Create())
        {
            var hash = await sha256.ComputeHashAsync(request.Content, cancellationToken).ConfigureAwait(false);
            checksum = Convert.ToHexStringLower(hash);
            request.Content.Position = 0;
        }

        var uploadResult = await storageProvider.UploadAsync(
            request.TenantId,
            blobName,
            request.Content,
            request.ContentType,
            new Dictionary<string, string>
            {
                ["originalFileName"] = request.OriginalFileName,
                ["category"] = request.Category.ToString(),
                ["uploadedBy"] = request.UploadedBy.ToString()
            },
            cancellationToken).ConfigureAwait(false);

        var storedFile = StoredFile.Create(
            request.TenantId,
            request.OriginalFileName,
            blobName,
            request.ContentType,
            uploadResult.ContentLength,
            request.Category,
            request.UploadedBy,
            request.EntityType,
            request.EntityId,
            request.Description,
            checksum);

        await repository.AddAsync(storedFile, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "File {FileId} uploaded by {UploadedBy} for tenant {TenantId}: {FileName} ({Size} bytes)",
            storedFile.Id, request.UploadedBy, request.TenantId,
            request.OriginalFileName, uploadResult.ContentLength);

        return Result<Guid>.Success(storedFile.Id);
    }

    private static string GenerateBlobName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        return $"{timestamp}/{Guid.NewGuid()}{extension}";
    }
}

[Auditable(AuditAction.Delete, AuditCategory.Storage, Severity = AuditSeverity.High)]
public sealed class DeleteFileCommandHandler(
    IStorageProvider storageProvider,
    IStoredFileRepository repository,
    ILogger<DeleteFileCommandHandler> logger) : IRequestHandler<DeleteFileCommand, Result>
{
    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var storedFile = await repository.GetByIdAsync(request.FileId, cancellationToken).ConfigureAwait(false);

        if (storedFile is null)
            return Result.Failure("File not found.", "FILE_NOT_FOUND");

        await storageProvider.DeleteAsync(request.TenantId, storedFile.BlobName, cancellationToken)
            .ConfigureAwait(false);

        storedFile.Delete();
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "File {FileId} deleted for tenant {TenantId}: {FileName}",
            request.FileId, request.TenantId, storedFile.OriginalFileName);

        return Result.Success();
    }
}

using HrSaas.Modules.Storage.Application.DTOs;
using HrSaas.Modules.Storage.Application.Queries;
using HrSaas.Modules.Storage.Domain.Repositories;
using HrSaas.SharedKernel.CQRS;
using HrSaas.SharedKernel.Pagination;
using HrSaas.SharedKernel.Storage;
using HrSaas.TenantSdk;
using MediatR;

namespace HrSaas.Modules.Storage.Application.Handlers;

public sealed class GetStoredFileByIdQueryHandler(
    IStoredFileRepository repository)
    : IRequestHandler<GetStoredFileByIdQuery, Result<StoredFileDetailDto>>
{
    public async Task<Result<StoredFileDetailDto>> Handle(
        GetStoredFileByIdQuery request, CancellationToken cancellationToken)
    {
        var file = await repository.GetByIdAsync(request.FileId, cancellationToken).ConfigureAwait(false);

        if (file is null)
            return Result<StoredFileDetailDto>.Failure("File not found.", "FILE_NOT_FOUND");

        return Result<StoredFileDetailDto>.Success(new StoredFileDetailDto(
            file.Id,
            file.OriginalFileName,
            file.BlobName,
            file.ContentType,
            file.SizeBytes,
            file.Category,
            file.Status,
            file.EntityType,
            file.EntityId,
            file.UploadedBy,
            file.Description,
            file.Checksum,
            file.CreatedAt,
            file.UpdatedAt));
    }
}

public sealed class ListStoredFilesQueryHandler(
    IStoredFileRepository repository)
    : IRequestHandler<ListStoredFilesQuery, Result<PagedResult<StoredFileDto>>>
{
    public async Task<Result<PagedResult<StoredFileDto>>> Handle(
        ListStoredFilesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.ListAsync(
            request.Page,
            request.PageSize,
            request.Category,
            request.Status,
            request.EntityType,
            request.EntityId,
            cancellationToken).ConfigureAwait(false);

        var dtos = items.Select(f => new StoredFileDto(
            f.Id,
            f.OriginalFileName,
            f.ContentType,
            f.SizeBytes,
            f.Category,
            f.Status,
            f.EntityType,
            f.EntityId,
            f.UploadedBy,
            f.Description,
            f.CreatedAt)).ToList().AsReadOnly();

        return Result<PagedResult<StoredFileDto>>.Success(
            new PagedResult<StoredFileDto>(dtos, request.Page, request.PageSize, totalCount));
    }
}

public sealed class GetFilesByEntityQueryHandler(
    IStoredFileRepository repository)
    : IRequestHandler<GetFilesByEntityQuery, Result<IReadOnlyList<StoredFileDto>>>
{
    public async Task<Result<IReadOnlyList<StoredFileDto>>> Handle(
        GetFilesByEntityQuery request, CancellationToken cancellationToken)
    {
        var files = await repository.GetByEntityAsync(
            request.EntityType, request.EntityId, cancellationToken).ConfigureAwait(false);

        var dtos = files.Select(f => new StoredFileDto(
            f.Id,
            f.OriginalFileName,
            f.ContentType,
            f.SizeBytes,
            f.Category,
            f.Status,
            f.EntityType,
            f.EntityId,
            f.UploadedBy,
            f.Description,
            f.CreatedAt)).ToList().AsReadOnly();

        return Result<IReadOnlyList<StoredFileDto>>.Success(dtos);
    }
}

public sealed class GeneratePresignedUrlQueryHandler(
    IStoredFileRepository repository,
    IStorageProvider storageProvider,
    ITenantService tenantService)
    : IRequestHandler<GeneratePresignedUrlQuery, Result<PresignedUrlDto>>
{
    public async Task<Result<PresignedUrlDto>> Handle(
        GeneratePresignedUrlQuery request, CancellationToken cancellationToken)
    {
        var file = await repository.GetByIdAsync(request.FileId, cancellationToken).ConfigureAwait(false);

        if (file is null)
            return Result<PresignedUrlDto>.Failure("File not found.", "FILE_NOT_FOUND");

        var expiry = TimeSpan.FromMinutes(request.ExpiryMinutes);
        var tenantId = tenantService.GetCurrentTenantId();

        var url = await storageProvider.GetPresignedUrlAsync(
            tenantId, file.BlobName, expiry, cancellationToken).ConfigureAwait(false);

        var expiresAt = DateTime.UtcNow.Add(expiry);

        return Result<PresignedUrlDto>.Success(new PresignedUrlDto(url, expiresAt));
    }
}

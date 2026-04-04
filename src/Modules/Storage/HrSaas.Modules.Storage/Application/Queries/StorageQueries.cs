using HrSaas.Modules.Storage.Application.DTOs;
using HrSaas.Modules.Storage.Domain.Enums;
using HrSaas.SharedKernel.CQRS;
using HrSaas.SharedKernel.Pagination;

namespace HrSaas.Modules.Storage.Application.Queries;

public sealed record GetStoredFileByIdQuery(Guid FileId) : IQuery<StoredFileDetailDto>;

public sealed record ListStoredFilesQuery(
    int Page,
    int PageSize,
    FileCategory? Category = null,
    FileStatus? Status = null,
    string? EntityType = null,
    string? EntityId = null) : IQuery<PagedResult<StoredFileDto>>;

public sealed record GetFilesByEntityQuery(
    string EntityType,
    string EntityId) : IQuery<IReadOnlyList<StoredFileDto>>;

public sealed record GeneratePresignedUrlQuery(
    Guid FileId,
    int ExpiryMinutes = 60) : IQuery<PresignedUrlDto>;

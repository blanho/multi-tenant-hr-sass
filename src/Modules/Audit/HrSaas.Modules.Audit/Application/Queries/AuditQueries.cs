using HrSaas.Modules.Audit.Application.DTOs;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.CQRS;
using HrSaas.SharedKernel.Pagination;

namespace HrSaas.Modules.Audit.Application.Queries;

public sealed record GetAuditLogsQuery(
    int Page = 1,
    int PageSize = 50,
    AuditCategory? Category = null,
    AuditAction? Action = null,
    Guid? UserId = null,
    string? EntityType = null,
    string? EntityId = null,
    DateTime? From = null,
    DateTime? To = null,
    bool? IsSuccess = null) : IQuery<PagedResult<AuditLogDto>>;

public sealed record GetAuditLogByIdQuery(Guid AuditLogId) : IQuery<AuditLogDetailDto>;

public sealed record GetAuditLogsForEntityQuery(
    string EntityType,
    string EntityId,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResult<AuditLogDto>>;

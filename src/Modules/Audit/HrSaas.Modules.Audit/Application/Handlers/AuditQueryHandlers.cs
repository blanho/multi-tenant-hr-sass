using HrSaas.Modules.Audit.Application.DTOs;
using HrSaas.Modules.Audit.Application.Queries;
using HrSaas.Modules.Audit.Infrastructure.Persistence;
using HrSaas.SharedKernel.CQRS;
using HrSaas.SharedKernel.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Audit.Application.Handlers;

public sealed class GetAuditLogsQueryHandler(AuditDbContext dbContext)
    : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
{
    public async Task<Result<PagedResult<AuditLogDto>>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs.AsNoTracking().AsQueryable();

        if (request.Category.HasValue)
            query = query.Where(a => a.Category == request.Category.Value);

        if (request.Action.HasValue)
            query = query.Where(a => a.Action == request.Action.Value);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (!string.IsNullOrWhiteSpace(request.EntityId))
            query = query.Where(a => a.EntityId == request.EntityId);

        if (request.From.HasValue)
            query = query.Where(a => a.Timestamp >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.Timestamp <= request.To.Value);

        if (request.IsSuccess.HasValue)
            query = query.Where(a => a.IsSuccess == request.IsSuccess.Value);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id, a.TenantId, a.UserId, a.UserEmail,
                a.Action, a.Category, a.Severity,
                a.EntityType, a.EntityId, a.CommandName,
                a.Description, a.IsSuccess, a.ErrorMessage,
                a.IpAddress, a.DurationMs, a.Timestamp))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<PagedResult<AuditLogDto>>.Success(
            new PagedResult<AuditLogDto>(items, request.Page, request.PageSize, totalCount));
    }
}

public sealed class GetAuditLogByIdQueryHandler(AuditDbContext dbContext)
    : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDetailDto>>
{
    public async Task<Result<AuditLogDetailDto>> Handle(
        GetAuditLogByIdQuery request,
        CancellationToken cancellationToken)
    {
        var log = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(a => a.Id == request.AuditLogId)
            .Select(a => new AuditLogDetailDto(
                a.Id, a.TenantId, a.UserId, a.UserEmail,
                a.Action, a.Category, a.Severity,
                a.EntityType, a.EntityId, a.CommandName,
                a.Description, a.Payload, a.OldValues, a.NewValues,
                a.IsSuccess, a.ErrorMessage, a.IpAddress,
                a.UserAgent, a.CorrelationId,
                a.DurationMs, a.Timestamp))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return log is not null
            ? Result<AuditLogDetailDto>.Success(log)
            : Result<AuditLogDetailDto>.Failure("Audit log entry not found.", "NOT_FOUND");
    }
}

public sealed class GetAuditLogsForEntityQueryHandler(AuditDbContext dbContext)
    : IRequestHandler<GetAuditLogsForEntityQuery, Result<PagedResult<AuditLogDto>>>
{
    public async Task<Result<PagedResult<AuditLogDto>>> Handle(
        GetAuditLogsForEntityQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id, a.TenantId, a.UserId, a.UserEmail,
                a.Action, a.Category, a.Severity,
                a.EntityType, a.EntityId, a.CommandName,
                a.Description, a.IsSuccess, a.ErrorMessage,
                a.IpAddress, a.DurationMs, a.Timestamp))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<PagedResult<AuditLogDto>>.Success(
            new PagedResult<AuditLogDto>(items, request.Page, request.PageSize, totalCount));
    }
}

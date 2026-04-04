using HrSaas.Modules.Notifications.Application.DTOs;
using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using HrSaas.SharedKernel.CQRS;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Notifications.Application.Queries;

public sealed record GetNotificationsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20,
    NotificationCategory? Category = null,
    NotificationChannel? Channel = null,
    bool? UnreadOnly = null) : IQuery<NotificationPagedResult>;

public sealed class GetNotificationsQueryHandler(INotificationsDbContext dbContext)
    : IRequestHandler<GetNotificationsQuery, Result<NotificationPagedResult>>
{
    public async Task<Result<NotificationPagedResult>> Handle(
        GetNotificationsQuery query,
        CancellationToken ct)
    {
        var baseQuery = dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == query.UserId);

        if (query.Category.HasValue)
            baseQuery = baseQuery.Where(n => n.Category == query.Category.Value);

        if (query.Channel.HasValue)
            baseQuery = baseQuery.Where(n => n.Channel == query.Channel.Value);

        if (query.UnreadOnly == true)
            baseQuery = baseQuery.Where(n => n.ReadAt == null && n.Status == NotificationStatus.Delivered);

        var totalCount = await baseQuery.CountAsync(ct).ConfigureAwait(false);

        var items = await baseQuery
            .OrderByDescending(n => n.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new NotificationDto(
                n.Id,
                n.UserId,
                n.Channel,
                n.Category,
                n.Priority,
                n.Subject,
                n.Body,
                n.Status,
                n.CreatedAt,
                n.ReadAt,
                n.DeliveredAt,
                n.CorrelationId))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return Result<NotificationPagedResult>.Success(
            new NotificationPagedResult(items.AsReadOnly(), query.Page, query.PageSize, totalCount));
    }
}

public sealed record GetUnreadCountQuery(Guid UserId) : IQuery<int>;

public sealed class GetUnreadCountQueryHandler(INotificationRepository repository)
    : IRequestHandler<GetUnreadCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(GetUnreadCountQuery query, CancellationToken ct)
    {
        var count = await repository.GetUnreadCountAsync(query.UserId, ct).ConfigureAwait(false);
        return Result<int>.Success(count);
    }
}

public sealed record GetNotificationByIdQuery(Guid NotificationId, Guid UserId) : IQuery<NotificationDetailDto>;

public sealed class GetNotificationByIdQueryHandler(INotificationsDbContext dbContext)
    : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDetailDto>>
{
    public async Task<Result<NotificationDetailDto>> Handle(
        GetNotificationByIdQuery query,
        CancellationToken ct)
    {
        var notification = await dbContext.Notifications
            .AsNoTracking()
            .Include(n => n.DeliveryAttempts)
            .Where(n => n.Id == query.NotificationId && n.UserId == query.UserId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (notification is null)
            return Result<NotificationDetailDto>.Failure("Notification not found.", "NOTIFICATION_NOT_FOUND");

        var dto = new NotificationDetailDto(
            notification.Id,
            notification.UserId,
            notification.Channel,
            notification.Category,
            notification.Priority,
            notification.Subject,
            notification.Body,
            notification.Status,
            notification.CreatedAt,
            notification.ReadAt,
            notification.DeliveredAt,
            notification.ScheduledAt,
            notification.ExpiresAt,
            notification.CorrelationId,
            notification.Metadata,
            notification.RecipientAddress,
            notification.RetryCount,
            notification.MaxRetries,
            notification.LastError,
            notification.DeliveryAttempts.Select(a => new DeliveryAttemptDto(
                a.Id,
                a.AttemptNumber,
                a.Status,
                a.ProviderResponse,
                a.ErrorMessage,
                a.AttemptedAt,
                a.CompletedAt,
                a.DurationMs)).ToList().AsReadOnly());

        return Result<NotificationDetailDto>.Success(dto);
    }
}

public sealed record GetUserPreferencesQuery(Guid UserId) : IQuery<IReadOnlyList<UserPreferenceDto>>;

public sealed class GetUserPreferencesQueryHandler(IUserNotificationPreferenceRepository repository)
    : IRequestHandler<GetUserPreferencesQuery, Result<IReadOnlyList<UserPreferenceDto>>>
{
    public async Task<Result<IReadOnlyList<UserPreferenceDto>>> Handle(
        GetUserPreferencesQuery query,
        CancellationToken ct)
    {
        var preferences = await repository.GetByUserIdAsync(query.UserId, ct).ConfigureAwait(false);

        var dtos = preferences.Select(p => new UserPreferenceDto(
            p.Id,
            p.UserId,
            p.Channel,
            p.Category,
            p.IsEnabled,
            p.DigestFrequency,
            p.QuietHoursStart,
            p.QuietHoursEnd,
            p.TimeZone)).ToList().AsReadOnly();

        return Result<IReadOnlyList<UserPreferenceDto>>.Success(dtos);
    }
}

public sealed record GetNotificationTemplatesQuery(
    NotificationCategory? Category = null,
    bool? ActiveOnly = null) : IQuery<IReadOnlyList<NotificationTemplateDto>>;

public sealed class GetNotificationTemplatesQueryHandler(INotificationsDbContext dbContext)
    : IRequestHandler<GetNotificationTemplatesQuery, Result<IReadOnlyList<NotificationTemplateDto>>>
{
    public async Task<Result<IReadOnlyList<NotificationTemplateDto>>> Handle(
        GetNotificationTemplatesQuery query,
        CancellationToken ct)
    {
        var baseQuery = dbContext.Templates.AsNoTracking();

        if (query.Category.HasValue)
            baseQuery = baseQuery.Where(t => t.Category == query.Category.Value);

        if (query.ActiveOnly == true)
            baseQuery = baseQuery.Where(t => t.IsActive);

        var templates = await baseQuery
            .OrderBy(t => t.Name)
            .Select(t => new NotificationTemplateDto(
                t.Id,
                t.Name,
                t.Slug,
                t.Channel,
                t.Category,
                t.SubjectTemplate,
                t.BodyTemplate,
                t.IsActive,
                t.Description))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<NotificationTemplateDto>>.Success(templates.AsReadOnly());
    }
}

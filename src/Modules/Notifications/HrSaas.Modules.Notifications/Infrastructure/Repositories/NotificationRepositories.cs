using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using HrSaas.Modules.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Notifications.Infrastructure.Repositories;

public sealed class NotificationRepository(NotificationsDbContext dbContext) : INotificationRepository
{
    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct).ConfigureAwait(false);

    public async Task<Notification?> GetByIdWithAttemptsAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Notifications
            .Include(n => n.DeliveryAttempts)
            .FirstOrDefaultAsync(n => n.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default) =>
        await dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(
        Guid userId, CancellationToken ct = default) =>
        await dbContext.Notifications
            .Where(n => n.UserId == userId && n.ReadAt == null
                        && (n.Status == NotificationStatus.Delivered || n.Status == NotificationStatus.Queued))
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default) =>
        await dbContext.Notifications
            .CountAsync(n => n.UserId == userId && n.ReadAt == null
                             && n.Status == NotificationStatus.Delivered, ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Notification>> GetFailedRetryableAsync(
        int batchSize, CancellationToken ct = default) =>
        await dbContext.Notifications
            .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < n.MaxRetries)
            .Where(n => !n.ExpiresAt.HasValue || n.ExpiresAt > DateTime.UtcNow)
            .OrderBy(n => n.UpdatedAt)
            .Take(batchSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Notification>> GetPendingScheduledAsync(
        DateTime cutoff, int batchSize, CancellationToken ct = default) =>
        await dbContext.Notifications
            .Where(n => n.Status == NotificationStatus.Pending
                        && n.ScheduledAt.HasValue && n.ScheduledAt <= cutoff)
            .OrderBy(n => n.ScheduledAt)
            .Take(batchSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task AddAsync(Notification notification, CancellationToken ct = default) =>
        await dbContext.Notifications.AddAsync(notification, ct).ConfigureAwait(false);

    public async Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken ct = default) =>
        await dbContext.Notifications.AddRangeAsync(notifications, ct).ConfigureAwait(false);

    public void Update(Notification notification) =>
        dbContext.Notifications.Update(notification);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

    public async Task<int> GetTotalCountAsync(
        Guid userId, NotificationCategory? category, CancellationToken ct = default)
    {
        var query = dbContext.Notifications.Where(n => n.UserId == userId);
        if (category.HasValue)
            query = query.Where(n => n.Category == category.Value);
        return await query.CountAsync(ct).ConfigureAwait(false);
    }
}

public sealed class NotificationTemplateRepository(NotificationsDbContext dbContext) : INotificationTemplateRepository
{
    public async Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Templates.FirstOrDefaultAsync(t => t.Id == id, ct).ConfigureAwait(false);

    public async Task<NotificationTemplate?> GetBySlugAsync(
        string slug, NotificationChannel channel, CancellationToken ct = default) =>
        await dbContext.Templates
            .FirstOrDefaultAsync(t => t.Slug == slug && t.Channel == channel && t.IsActive, ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<NotificationTemplate>> GetAllActiveAsync(CancellationToken ct = default) =>
        await dbContext.Templates.Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<NotificationTemplate>> GetByCategoryAsync(
        NotificationCategory category, CancellationToken ct = default) =>
        await dbContext.Templates
            .Where(t => t.Category == category && t.IsActive)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task AddAsync(NotificationTemplate template, CancellationToken ct = default) =>
        await dbContext.Templates.AddAsync(template, ct).ConfigureAwait(false);

    public void Update(NotificationTemplate template) =>
        dbContext.Templates.Update(template);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}

public sealed class UserNotificationPreferenceRepository(NotificationsDbContext dbContext)
    : IUserNotificationPreferenceRepository
{
    public async Task<UserNotificationPreference?> GetAsync(
        Guid userId, NotificationChannel channel, NotificationCategory category, CancellationToken ct = default) =>
        await dbContext.Preferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Channel == channel && p.Category == category, ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<UserNotificationPreference>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default) =>
        await dbContext.Preferences
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Channel).ThenBy(p => p.Category)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task AddAsync(UserNotificationPreference preference, CancellationToken ct = default) =>
        await dbContext.Preferences.AddAsync(preference, ct).ConfigureAwait(false);

    public async Task AddRangeAsync(IEnumerable<UserNotificationPreference> preferences, CancellationToken ct = default) =>
        await dbContext.Preferences.AddRangeAsync(preferences, ct).ConfigureAwait(false);

    public void Update(UserNotificationPreference preference) =>
        dbContext.Preferences.Update(preference);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}

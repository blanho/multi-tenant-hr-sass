using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;

namespace HrSaas.Modules.Notifications.Domain.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Notification?> GetByIdWithAttemptsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetFailedRetryableAsync(int batchSize, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetPendingScheduledAsync(DateTime cutoff, int batchSize, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken ct = default);
    void Update(Notification notification);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid userId, NotificationCategory? category, CancellationToken ct = default);
}

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<NotificationTemplate?> GetBySlugAsync(string slug, NotificationChannel channel, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationTemplate>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<NotificationTemplate>> GetByCategoryAsync(NotificationCategory category, CancellationToken ct = default);
    Task AddAsync(NotificationTemplate template, CancellationToken ct = default);
    void Update(NotificationTemplate template);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IUserNotificationPreferenceRepository
{
    Task<UserNotificationPreference?> GetAsync(Guid userId, NotificationChannel channel, NotificationCategory category, CancellationToken ct = default);
    Task<IReadOnlyList<UserNotificationPreference>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(UserNotificationPreference preference, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<UserNotificationPreference> preferences, CancellationToken ct = default);
    void Update(UserNotificationPreference preference);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Notifications.Application.Interfaces;

public interface INotificationsDbContext
{
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationTemplate> Templates { get; }
    DbSet<UserNotificationPreference> Preferences { get; }
    DbSet<DeliveryAttempt> DeliveryAttempts { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

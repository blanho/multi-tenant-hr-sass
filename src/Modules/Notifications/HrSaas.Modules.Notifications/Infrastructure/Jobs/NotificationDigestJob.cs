using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Infrastructure.Persistence;
using HrSaas.SharedKernel.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrSaas.Modules.Notifications.Infrastructure.Jobs;

public sealed class NotificationDigestJob(
    NotificationsDbContext dbContext,
    ILogger<NotificationDigestJob> logger) : IRecurringJob
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        var dailyDigestUsers = await dbContext.Preferences
            .IgnoreQueryFilters()
            .Where(p => p.DigestFrequency == DigestFrequency.Daily && p.IsEnabled)
            .Select(p => new { p.TenantId, p.UserId })
            .Distinct()
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (dailyDigestUsers.Count == 0) return;

        var digestCount = 0;

        foreach (var user in dailyDigestUsers)
        {
            var unreadCount = await dbContext.Notifications
                .IgnoreQueryFilters()
                .Where(n => n.TenantId == user.TenantId
                    && n.UserId == user.UserId
                    && n.Channel == NotificationChannel.InApp
                    && n.Status != NotificationStatus.Read
                    && n.Status != NotificationStatus.Cancelled
                    && n.CreatedAt >= cutoff)
                .CountAsync(ct)
                .ConfigureAwait(false);

            if (unreadCount == 0) continue;

            var digest = Notification.Create(
                user.TenantId,
                user.UserId,
                NotificationChannel.InApp,
                NotificationCategory.System,
                NotificationPriority.Normal,
                $"Daily Digest: {unreadCount} unread notifications",
                $"You have {unreadCount} unread notifications from the past 24 hours. Review your notification center for details.",
                null,
                null);

            await dbContext.Notifications.AddAsync(digest, ct).ConfigureAwait(false);
            digestCount++;
        }

        if (digestCount > 0)
        {
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            logger.LogInformation(
                "Created {DigestCount} daily digest notifications for {UserCount} eligible users",
                digestCount, dailyDigestUsers.Count);
        }
    }
}

using HrSaas.Modules.Notifications.Application.Interfaces;
using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext(
    DbContextOptions<NotificationsDbContext> options,
    TenantContext tenantContext,
    IPublisher publisher)
    : DbContext(options), INotificationsDbContext
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> Templates => Set<NotificationTemplate>();
    public DbSet<UserNotificationPreference> Preferences => Set<UserNotificationPreference>();
    public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("notification");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);

        modelBuilder.Entity<Notification>()
            .HasQueryFilter(n => n.TenantId == tenantContext.TenantId && !n.IsDeleted);

        modelBuilder.Entity<NotificationTemplate>()
            .HasQueryFilter(t => t.TenantId == tenantContext.TenantId && !t.IsDeleted);

        modelBuilder.Entity<UserNotificationPreference>()
            .HasQueryFilter(p => p.TenantId == tenantContext.TenantId && !p.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var result = await base.SaveChangesAsync(ct).ConfigureAwait(false);
        await DispatchDomainEventsAsync(ct).ConfigureAwait(false);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var entities = ChangeTracker
            .Entries<SharedKernel.Entities.BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var events = entities.SelectMany(e => e.DomainEvents).ToList();
        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in events)
        {
            await publisher.Publish(domainEvent, ct).ConfigureAwait(false);
        }
    }
}

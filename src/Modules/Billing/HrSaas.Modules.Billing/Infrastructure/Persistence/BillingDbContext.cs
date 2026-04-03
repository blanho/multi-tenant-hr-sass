using HrSaas.Modules.Billing.Domain.Entities;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Billing.Infrastructure.Persistence;

public sealed class BillingDbContext(
    DbContextOptions<BillingDbContext> options,
    TenantContext tenantContext,
    IPublisher publisher)
    : DbContext(options)
{
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
        modelBuilder.Entity<Subscription>()
            .HasQueryFilter(s => s.TenantId == tenantContext.TenantId && !s.IsDeleted);
        base.OnModelCreating(modelBuilder);
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

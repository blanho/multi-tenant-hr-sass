using MediatR;
using TenantEntity = HrSaas.Modules.Tenant.Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Tenant.Infrastructure.Persistence;

public sealed class TenantDbContext(
    DbContextOptions<TenantDbContext> options,
    IPublisher publisher)
    : DbContext(options)
{
    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tenant");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
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

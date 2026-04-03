using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options,
    TenantContext tenantContext,
    IPublisher publisher)
    : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        modelBuilder.Entity<AppUser>()
            .HasQueryFilter(u => u.TenantId == tenantContext.TenantId && !u.IsDeleted);
        modelBuilder.Entity<Role>()
            .HasQueryFilter(r => r.TenantId == tenantContext.TenantId && !r.IsDeleted);
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

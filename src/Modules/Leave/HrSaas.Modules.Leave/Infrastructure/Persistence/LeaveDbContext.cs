using HrSaas.Modules.Leave.Domain.Entities;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Leave.Infrastructure.Persistence;

public sealed class LeaveDbContext(
    DbContextOptions<LeaveDbContext> options,
    TenantContext tenantContext,
    IPublisher publisher)
    : DbContext(options)
{
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("leave");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeaveDbContext).Assembly);
        modelBuilder.Entity<LeaveRequest>().HasQueryFilter(l => l.TenantId == tenantContext.TenantId && !l.IsDeleted);
        modelBuilder.Entity<LeaveBalance>().HasQueryFilter(b => b.TenantId == tenantContext.TenantId);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await DispatchDomainEventsAsync(cancellationToken).ConfigureAwait(false);
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

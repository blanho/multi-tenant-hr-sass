using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Employee.Infrastructure.Persistence;

public sealed class EmployeeDbContext(
    DbContextOptions<EmployeeDbContext> options,
    TenantContext tenantContext,
    IPublisher publisher)
    : DbContext(options), IEmployeeDbContext
{
    public DbSet<Domain.Entities.Employee> Employees => Set<Domain.Entities.Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("employee");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeeDbContext).Assembly);
        modelBuilder.Entity<Domain.Entities.Employee>()
            .HasQueryFilter(e => e.TenantId == tenantContext.TenantId && !e.IsDeleted);
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

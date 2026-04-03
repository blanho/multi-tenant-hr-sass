using HrSaas.Modules.Employee.Domain.Entities;
using HrSaas.TenantSdk;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Employee.Infrastructure.Persistence;

public sealed class EmployeeDbContext(
    DbContextOptions<EmployeeDbContext> options,
    ITenantService tenantService,
    IMediator mediator)
    : DbContext(options), IEmployeeDbContext
{
    public DbSet<Domain.Entities.Employee> Employees => Set<Domain.Entities.Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("employee");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeeDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await DispatchDomainEventsAsync(cancellationToken).ConfigureAwait(false);

        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<SharedKernel.Entities.BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent, ct).ConfigureAwait(false);
    }
}

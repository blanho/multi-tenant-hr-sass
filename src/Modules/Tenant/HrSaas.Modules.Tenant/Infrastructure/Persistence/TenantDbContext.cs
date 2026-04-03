using TenantEntity = HrSaas.Modules.Tenant.Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Tenant.Infrastructure.Persistence;

public sealed class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
{
    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tenant");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

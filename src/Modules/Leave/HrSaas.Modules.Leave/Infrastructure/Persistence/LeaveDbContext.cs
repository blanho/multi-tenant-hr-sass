using HrSaas.Modules.Leave.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Leave.Infrastructure.Persistence;

public sealed class LeaveDbContext(DbContextOptions<LeaveDbContext> options) : DbContext(options)
{
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("leave");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeaveDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

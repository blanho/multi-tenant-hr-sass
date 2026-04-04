using HrSaas.Modules.Audit.Domain.Entities;
using HrSaas.TenantSdk;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Audit.Infrastructure.Persistence;

public sealed class AuditDbContext(
    DbContextOptions<AuditDbContext> options,
    TenantContext tenantContext)
    : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("audit");

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Action).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Category).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Severity).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(256);
            entity.Property(e => e.EntityId).HasMaxLength(256);
            entity.Property(e => e.CommandName).IsRequired().HasMaxLength(512);
            entity.Property(e => e.Description).HasMaxLength(1024);
            entity.Property(e => e.Payload).HasColumnType("jsonb");
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
            entity.Property(e => e.ErrorMessage).HasMaxLength(2048);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(512);
            entity.Property(e => e.UserEmail).HasMaxLength(254);
            entity.Property(e => e.CorrelationId).HasMaxLength(128);
            entity.Property(e => e.Timestamp).IsRequired();

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.TenantId, e.Timestamp });
            entity.HasIndex(e => new { e.TenantId, e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.TenantId, e.UserId });
            entity.HasIndex(e => new { e.TenantId, e.Category });
            entity.HasIndex(e => e.CorrelationId);

            entity.HasQueryFilter(e => e.TenantId == tenantContext.TenantId);
        });
    }
}

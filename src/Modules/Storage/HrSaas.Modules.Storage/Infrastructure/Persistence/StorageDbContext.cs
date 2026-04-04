using HrSaas.Modules.Storage.Domain.Entities;
using HrSaas.TenantSdk;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Storage.Infrastructure.Persistence;

public sealed class StorageDbContext(
    DbContextOptions<StorageDbContext> options,
    TenantContext tenantContext)
    : DbContext(options)
{
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("storage");

        modelBuilder.Entity<StoredFile>(entity =>
        {
            entity.ToTable("stored_files");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(512);
            entity.Property(e => e.BlobName).IsRequired().HasMaxLength(1024);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(256);
            entity.Property(e => e.SizeBytes).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.EntityType).HasMaxLength(256);
            entity.Property(e => e.EntityId).HasMaxLength(256);
            entity.Property(e => e.UploadedBy).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2048);
            entity.Property(e => e.Checksum).HasMaxLength(128);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.Category });
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.TenantId, e.UploadedBy });
            entity.HasIndex(e => e.BlobName).IsUnique();
            entity.HasIndex(e => e.CreatedAt);

            entity.HasQueryFilter(e =>
                e.TenantId == tenantContext.TenantId && !e.IsDeleted);
        });
    }
}

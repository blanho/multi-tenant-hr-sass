using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.TenantSdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.TenantId).IsRequired();
        builder.Property(r => r.Name).HasMaxLength(64).IsRequired();
        builder.Property(r => r.IsSystemRole).IsRequired();
        builder.PrimitiveCollection(r => r.Permissions).ElementType().HasMaxLength(128);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt);
        builder.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();
    }
}

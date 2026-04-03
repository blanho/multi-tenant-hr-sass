using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.Modules.Identity.Domain.ValueObjects;
using HrSaas.TenantSdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.TenantId).IsRequired();
        builder.OwnsOne(u => u.Email, e => {
            e.Property(x => x.Value).HasColumnName("email").HasMaxLength(254).IsRequired();
        });
        builder.OwnsOne(u => u.Password, p => {
            p.Property(x => x.Value).HasColumnName("password_hash").HasMaxLength(512).IsRequired();
        });
        builder.Property(u => u.Role).HasMaxLength(32).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);
        builder.HasIndex(u => new { u.TenantId, u.Id }).IsUnique();
        builder.HasIndex("TenantId", "email").IsUnique();
    }
}

using HrSaas.TenantSdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Employee.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration(ITenantService tenantService)
    : IEntityTypeConfiguration<Domain.Entities.Employee>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Employee> builder)
    {
        builder.ToTable("employees", "employee");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.OwnsOne(e => e.Department, dept =>
        {
            dept.Property(d => d.Name)
                .HasColumnName("department")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(e => e.Position, pos =>
        {
            pos.Property(p => p.Title)
                .HasColumnName("position")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("idx_employees_tenant_id");

        builder.HasIndex(e => new { e.TenantId, e.Email })
            .IsUnique()
            .HasDatabaseName("idx_employees_tenant_email_unique");

        builder.HasQueryFilter(e =>
            !e.IsDeleted &&
            e.TenantId == tenantService.GetCurrentTenantId());

        builder.Ignore(e => e.DomainEvents);
    }
}

using HrSaas.Modules.Leave.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Leave.Infrastructure.Persistence.Configurations;

public sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("leave_balances");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Property(b => b.TenantId).IsRequired();
        builder.Property(b => b.EmployeeId).IsRequired();
        builder.Property(b => b.Year).IsRequired();
        builder.Property(b => b.AnnualAllowance).IsRequired();
        builder.Property(b => b.SickAllowance).IsRequired();
        builder.Property(b => b.AnnualUsed).IsRequired();
        builder.Property(b => b.SickUsed).IsRequired();
        builder.HasIndex(b => new { b.TenantId, b.EmployeeId, b.Year }).IsUnique();
    }
}

using HrSaas.Modules.Leave.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Leave.Infrastructure.Persistence.Configurations;

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("leave_requests");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();
        builder.Property(l => l.TenantId).IsRequired();
        builder.Property(l => l.EmployeeId).IsRequired();
        builder.Property(l => l.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(l => l.StartDate).IsRequired();
        builder.Property(l => l.EndDate).IsRequired();
        builder.Property(l => l.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(l => l.RejectionNote).HasMaxLength(1000);
        builder.HasQueryFilter(l => l.TenantId == l.TenantId && !l.IsDeleted);
        builder.HasIndex(l => new { l.TenantId, l.EmployeeId });
        builder.HasIndex(l => new { l.TenantId, l.Status });
    }
}

using HrSaas.Modules.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Billing.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.TenantId).IsRequired();
        builder.Property(s => s.PlanName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(s => s.Cycle).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(s => s.PricePerCycle).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.MaxSeats).IsRequired();
        builder.Property(s => s.UsedSeats).IsRequired();
        builder.Property(s => s.ExternalSubscriptionId).HasMaxLength(200);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.HasIndex(s => s.TenantId);
    }
}

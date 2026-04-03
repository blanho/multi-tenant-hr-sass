using HrSaas.SharedKernel.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.EventBus.Outbox;

public sealed class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("outbox");
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).IsRequired().HasMaxLength(512);
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.Error).HasMaxLength(2000);
        builder.Property(m => m.ProcessedAt);
        builder.Property(m => m.RetryCount);
        builder.HasIndex(m => m.ProcessedAt).HasDatabaseName("ix_outbox_messages_processed_at");
        builder.HasIndex(m => new { m.TenantId, m.OccurredAt }).HasDatabaseName("ix_outbox_messages_tenant_occurred");
    }
}

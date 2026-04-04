using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", "notification");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.TenantId).IsRequired();
        builder.Property(n => n.UserId).IsRequired();

        builder.Property(n => n.Channel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.Subject).HasMaxLength(500).IsRequired();
        builder.Property(n => n.Body).IsRequired();
        builder.Property(n => n.RecipientAddress).HasMaxLength(500);
        builder.Property(n => n.CorrelationId).HasMaxLength(200);
        builder.Property(n => n.Metadata).HasColumnType("jsonb");
        builder.Property(n => n.LastError).HasMaxLength(2000);
        builder.Property(n => n.MaxRetries).HasDefaultValue(3);

        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.IsDeleted).HasDefaultValue(false);

        builder.HasMany(n => n.DeliveryAttempts)
            .WithOne()
            .HasForeignKey(a => a.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.TenantId)
            .HasDatabaseName("idx_notifications_tenant_id");

        builder.HasIndex(n => new { n.TenantId, n.UserId })
            .HasDatabaseName("idx_notifications_tenant_user");

        builder.HasIndex(n => new { n.TenantId, n.UserId, n.Status })
            .HasDatabaseName("idx_notifications_tenant_user_status");

        builder.HasIndex(n => new { n.TenantId, n.Status, n.ScheduledAt })
            .HasDatabaseName("idx_notifications_scheduled")
            .HasFilter("\"ScheduledAt\" IS NOT NULL");

        builder.HasIndex(n => new { n.TenantId, n.Status, n.RetryCount })
            .HasDatabaseName("idx_notifications_retryable")
            .HasFilter("\"Status\" = 'Failed'");

        builder.HasIndex(n => n.CorrelationId)
            .HasDatabaseName("idx_notifications_correlation");

        builder.HasIndex(n => n.GroupId)
            .HasDatabaseName("idx_notifications_group")
            .HasFilter("\"GroupId\" IS NOT NULL");

        builder.HasIndex(n => n.CreatedAt)
            .HasDatabaseName("idx_notifications_created_at")
            .IsDescending();
    }
}

public sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates", "notification");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.TenantId).IsRequired();

        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(200).IsRequired();

        builder.Property(t => t.Channel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.SubjectTemplate).HasMaxLength(500).IsRequired();
        builder.Property(t => t.BodyTemplate).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.SamplePayload).HasColumnType("jsonb");
        builder.Property(t => t.IsActive).HasDefaultValue(true);
        builder.Property(t => t.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(t => new { t.TenantId, t.Slug, t.Channel })
            .IsUnique()
            .HasDatabaseName("idx_templates_tenant_slug_channel");

        builder.HasIndex(t => new { t.TenantId, t.Category })
            .HasDatabaseName("idx_templates_tenant_category");
    }
}

public sealed class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.ToTable("user_notification_preferences", "notification");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.UserId).IsRequired();

        builder.Property(p => p.Channel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.DigestFrequency)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(DigestFrequency.Immediate);

        builder.Property(p => p.IsEnabled).HasDefaultValue(true);
        builder.Property(p => p.TimeZone).HasMaxLength(100).HasDefaultValue("UTC");
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(p => new { p.TenantId, p.UserId, p.Channel, p.Category })
            .IsUnique()
            .HasDatabaseName("idx_preferences_user_channel_category");
    }
}

public sealed class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("delivery_attempts", "notification");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.NotificationId).IsRequired();
        builder.Property(a => a.AttemptNumber).IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.ProviderResponse).HasMaxLength(2000);
        builder.Property(a => a.ErrorMessage).HasMaxLength(2000);
        builder.Property(a => a.AttemptedAt).IsRequired();

        builder.HasIndex(a => a.NotificationId)
            .HasDatabaseName("idx_delivery_attempts_notification");
    }
}

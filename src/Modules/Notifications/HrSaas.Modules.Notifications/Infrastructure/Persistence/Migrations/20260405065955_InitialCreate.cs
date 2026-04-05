using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSaas.Modules.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notification");

            migrationBuilder.CreateTable(
                name: "notification_templates",
                schema: "notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubjectTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BodyTemplate = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SamplePayload = table.Column<string>(type: "jsonb", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    RecipientAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_notification_preferences",
                schema: "notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DigestFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Immediate"),
                    QuietHoursStart = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    QuietHoursEnd = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValue: "UTC"),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notification_preferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "delivery_attempts",
                schema: "notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProviderResponse = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_delivery_attempts_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "notification",
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_delivery_attempts_notification",
                schema: "notification",
                table: "delivery_attempts",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "idx_templates_tenant_category",
                schema: "notification",
                table: "notification_templates",
                columns: new[] { "TenantId", "Category" });

            migrationBuilder.CreateIndex(
                name: "idx_templates_tenant_slug_channel",
                schema: "notification",
                table: "notification_templates",
                columns: new[] { "TenantId", "Slug", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_notifications_correlation",
                schema: "notification",
                table: "notifications",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "idx_notifications_created_at",
                schema: "notification",
                table: "notifications",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_notifications_group",
                schema: "notification",
                table: "notifications",
                column: "GroupId",
                filter: "\"GroupId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_notifications_retryable",
                schema: "notification",
                table: "notifications",
                columns: new[] { "TenantId", "Status", "RetryCount" },
                filter: "\"Status\" = 'Failed'");

            migrationBuilder.CreateIndex(
                name: "idx_notifications_scheduled",
                schema: "notification",
                table: "notifications",
                columns: new[] { "TenantId", "Status", "ScheduledAt" },
                filter: "\"ScheduledAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_notifications_tenant_id",
                schema: "notification",
                table: "notifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "idx_notifications_tenant_user",
                schema: "notification",
                table: "notifications",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "idx_notifications_tenant_user_status",
                schema: "notification",
                table: "notifications",
                columns: new[] { "TenantId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_preferences_user_channel_category",
                schema: "notification",
                table: "user_notification_preferences",
                columns: new[] { "TenantId", "UserId", "Channel", "Category" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delivery_attempts",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "notification_templates",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "user_notification_preferences",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "notification");
        }
    }
}

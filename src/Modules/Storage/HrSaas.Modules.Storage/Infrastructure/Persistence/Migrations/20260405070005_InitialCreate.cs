using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSaas.Modules.Storage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "storage");

            migrationBuilder.CreateTable(
                name: "stored_files",
                schema: "storage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    BlobName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EntityId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Checksum = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stored_files", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_BlobName",
                schema: "storage",
                table: "stored_files",
                column: "BlobName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_CreatedAt",
                schema: "storage",
                table: "stored_files",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_TenantId",
                schema: "storage",
                table: "stored_files",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_TenantId_Category",
                schema: "storage",
                table: "stored_files",
                columns: new[] { "TenantId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_TenantId_EntityType_EntityId",
                schema: "storage",
                table: "stored_files",
                columns: new[] { "TenantId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_TenantId_Status",
                schema: "storage",
                table: "stored_files",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_TenantId_UploadedBy",
                schema: "storage",
                table: "stored_files",
                columns: new[] { "TenantId", "UploadedBy" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stored_files",
                schema: "storage");
        }
    }
}

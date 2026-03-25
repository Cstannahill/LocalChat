using System;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260319050000_AddRuntimeSourceAndAppDefaults")]
    public partial class AddRuntimeSourceAndAppDefaults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RuntimeGenerationPresetOverrideId",
                table: "Conversations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RuntimeModelProfileOverrideId",
                table: "Conversations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RuntimeSourceType",
                table: "MessageVariants",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppRuntimeDefaults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DefaultUserProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefaultModelProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefaultGenerationPresetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRuntimeDefaults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_RuntimeGenerationPresetOverrideId",
                table: "Conversations",
                column: "RuntimeGenerationPresetOverrideId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_RuntimeModelProfileOverrideId",
                table: "Conversations",
                column: "RuntimeModelProfileOverrideId");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRuntimeDefaults");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_RuntimeGenerationPresetOverrideId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_RuntimeModelProfileOverrideId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "RuntimeGenerationPresetOverrideId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "RuntimeModelProfileOverrideId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "RuntimeSourceType",
                table: "MessageVariants");
        }
    }
}

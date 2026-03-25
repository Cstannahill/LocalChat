using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageVariantGenerationProvenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GenerationPresetId",
                table: "MessageVariants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelIdentifier",
                table: "MessageVariants",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModelProfileId",
                table: "MessageVariants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProviderType",
                table: "MessageVariants",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageVariants_MessageId",
                table: "MessageVariants",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageVariants_ModelIdentifier",
                table: "MessageVariants",
                column: "ModelIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_MessageVariants_ProviderType",
                table: "MessageVariants",
                column: "ProviderType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageVariants_MessageId",
                table: "MessageVariants");

            migrationBuilder.DropIndex(
                name: "IX_MessageVariants_ModelIdentifier",
                table: "MessageVariants");

            migrationBuilder.DropIndex(
                name: "IX_MessageVariants_ProviderType",
                table: "MessageVariants");

            migrationBuilder.DropColumn(
                name: "GenerationPresetId",
                table: "MessageVariants");

            migrationBuilder.DropColumn(
                name: "ModelIdentifier",
                table: "MessageVariants");

            migrationBuilder.DropColumn(
                name: "ModelProfileId",
                table: "MessageVariants");

            migrationBuilder.DropColumn(
                name: "ProviderType",
                table: "MessageVariants");
        }
    }
}

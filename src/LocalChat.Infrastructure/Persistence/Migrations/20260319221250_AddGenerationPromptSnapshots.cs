using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGenerationPromptSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GenerationPromptSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageVariantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullPromptText = table.Column<string>(type: "TEXT", nullable: false),
                    PromptSectionsJson = table.Column<string>(type: "TEXT", nullable: false),
                    EstimatedPromptTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    ResolvedContextWindow = table.Column<int>(type: "INTEGER", nullable: true),
                    ProviderType = table.Column<int>(type: "INTEGER", nullable: true),
                    ModelIdentifier = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ModelProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    GenerationPresetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RuntimeSourceType = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationPromptSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenerationPromptSnapshots_ConversationId",
                table: "GenerationPromptSnapshots",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationPromptSnapshots_CreatedAt",
                table: "GenerationPromptSnapshots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationPromptSnapshots_MessageId",
                table: "GenerationPromptSnapshots",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationPromptSnapshots_MessageVariantId",
                table: "GenerationPromptSnapshots",
                column: "MessageVariantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenerationPromptSnapshots_ModelIdentifier",
                table: "GenerationPromptSnapshots",
                column: "ModelIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationPromptSnapshots_ProviderType",
                table: "GenerationPromptSnapshots",
                column: "ProviderType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenerationPromptSnapshots");
        }
    }
}

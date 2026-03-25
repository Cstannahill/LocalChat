using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSceneStateExtractionEventsAndSlotFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SceneStateExtractionEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SlotFamily = table.Column<string>(type: "TEXT", nullable: false),
                    SlotKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CandidateContent = table.Column<string>(type: "TEXT", nullable: false),
                    CandidateNormalizedKey = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ConfidenceScore = table.Column<double>(type: "REAL", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ReplacedMemoryItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReplacedMemoryContent = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SceneStateExtractionEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SceneStateExtractionEvents_Action",
                table: "SceneStateExtractionEvents",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_SceneStateExtractionEvents_CharacterId",
                table: "SceneStateExtractionEvents",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_SceneStateExtractionEvents_ConversationId",
                table: "SceneStateExtractionEvents",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_SceneStateExtractionEvents_CreatedAt",
                table: "SceneStateExtractionEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SceneStateExtractionEvents_SlotFamily",
                table: "SceneStateExtractionEvents",
                column: "SlotFamily");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SceneStateExtractionEvents");
        }
    }
}

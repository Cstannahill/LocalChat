using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionStateExtractionEventsAndSlotFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionStateExtractionEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgentId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_SessionStateExtractionEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionStateExtractionEvents_Action",
                table: "SessionStateExtractionEvents",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_SessionStateExtractionEvents_AgentId",
                table: "SessionStateExtractionEvents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionStateExtractionEvents_ConversationId",
                table: "SessionStateExtractionEvents",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionStateExtractionEvents_CreatedAt",
                table: "SessionStateExtractionEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionStateExtractionEvents_SlotFamily",
                table: "SessionStateExtractionEvents",
                column: "SlotFamily");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionStateExtractionEvents");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemoryExtractionAuditEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemoryExtractionAuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    SlotFamily = table.Column<string>(type: "TEXT", nullable: false),
                    SlotKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CandidateContent = table.Column<string>(type: "TEXT", nullable: false),
                    CandidateNormalizedKey = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ConfidenceScore = table.Column<double>(type: "REAL", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExistingMemoryItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ExistingMemoryContent = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryExtractionAuditEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemoryExtractionAuditEvents_Action",
                table: "MemoryExtractionAuditEvents",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryExtractionAuditEvents_AgentId",
                table: "MemoryExtractionAuditEvents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryExtractionAuditEvents_ConversationId",
                table: "MemoryExtractionAuditEvents",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryExtractionAuditEvents_CreatedAt",
                table: "MemoryExtractionAuditEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryExtractionAuditEvents_Kind",
                table: "MemoryExtractionAuditEvents",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryExtractionAuditEvents_SlotFamily",
                table: "MemoryExtractionAuditEvents",
                column: "SlotFamily");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemoryExtractionAuditEvents");
        }
    }
}

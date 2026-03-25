using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemoryOperationAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemoryOperationAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemoryItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceMemoryItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TargetMemoryItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OperationType = table.Column<string>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AgentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MessageSequenceNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    BeforeStateJson = table.Column<string>(type: "TEXT", nullable: true),
                    AfterStateJson = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsUndone = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    UndoneAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UndoAuditId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryOperationAudits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemoryOperationAudits_CreatedAt",
                table: "MemoryOperationAudits",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryOperationAudits_MemoryItemId",
                table: "MemoryOperationAudits",
                column: "MemoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryOperationAudits_OperationType",
                table: "MemoryOperationAudits",
                column: "OperationType");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryOperationAudits_SourceMemoryItemId",
                table: "MemoryOperationAudits",
                column: "SourceMemoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryOperationAudits_TargetMemoryItemId",
                table: "MemoryOperationAudits",
                column: "TargetMemoryItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemoryOperationAudits");
        }
    }
}

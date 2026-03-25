using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemorySlotKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SlotKey",
                table: "MemoryItems",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_AgentId_ConversationId_Kind_SlotKey",
                table: "MemoryItems",
                columns: new[] { "AgentId", "ConversationId", "Kind", "SlotKey" });

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_SlotKey",
                table: "MemoryItems",
                column: "SlotKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_AgentId_ConversationId_Kind_SlotKey",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_SlotKey",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "SlotKey",
                table: "MemoryItems");
        }
    }
}

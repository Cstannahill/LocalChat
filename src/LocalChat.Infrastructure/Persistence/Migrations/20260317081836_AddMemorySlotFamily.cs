using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemorySlotFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SlotFamily",
                table: "MemoryItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "None");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_CharacterId_ConversationId_Kind_SlotFamily",
                table: "MemoryItems",
                columns: new[] { "CharacterId", "ConversationId", "Kind", "SlotFamily" });

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_SlotFamily",
                table: "MemoryItems",
                column: "SlotFamily");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_CharacterId_ConversationId_Kind_SlotFamily",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_SlotFamily",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "SlotFamily",
                table: "MemoryItems");
        }
    }
}

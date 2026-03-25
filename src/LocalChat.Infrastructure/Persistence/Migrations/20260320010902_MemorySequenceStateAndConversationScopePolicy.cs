using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MemorySequenceStateAndConversationScopePolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastObservedSequenceNumber",
                table: "MemoryItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScopeType",
                table: "MemoryItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "Conversation");

            migrationBuilder.AddColumn<int>(
                name: "SourceMessageSequenceNumber",
                table: "MemoryItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupersededAtSequenceNumber",
                table: "MemoryItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_LastObservedSequenceNumber",
                table: "MemoryItems",
                column: "LastObservedSequenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_ScopeType",
                table: "MemoryItems",
                column: "ScopeType");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_SourceMessageSequenceNumber",
                table: "MemoryItems",
                column: "SourceMessageSequenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_SupersededAtSequenceNumber",
                table: "MemoryItems",
                column: "SupersededAtSequenceNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_LastObservedSequenceNumber",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_ScopeType",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_SourceMessageSequenceNumber",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_SupersededAtSequenceNumber",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "LastObservedSequenceNumber",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "ScopeType",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "SourceMessageSequenceNumber",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "SupersededAtSequenceNumber",
                table: "MemoryItems");
        }
    }
}

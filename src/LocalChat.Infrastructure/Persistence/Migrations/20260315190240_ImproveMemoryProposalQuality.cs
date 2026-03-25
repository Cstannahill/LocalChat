using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImproveMemoryProposalQuality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_IsPinned",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "IsDerived",
                table: "MemoryItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "AgentId",
                table: "MemoryItems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "ConflictsWithMemoryItemId",
                table: "MemoryItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedKey",
                table: "MemoryItems",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProposalReason",
                table: "MemoryItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceExcerpt",
                table: "MemoryItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_Category",
                table: "MemoryItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_NormalizedKey",
                table: "MemoryItems",
                column: "NormalizedKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_Category",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_NormalizedKey",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "ConflictsWithMemoryItemId",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "NormalizedKey",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "ProposalReason",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "SourceExcerpt",
                table: "MemoryItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "AgentId",
                table: "MemoryItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDerived",
                table: "MemoryItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_IsPinned",
                table: "MemoryItems",
                column: "IsPinned");
        }
    }
}

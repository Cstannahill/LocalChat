using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemoryKindAndExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemoryItems_Characters_CharacterId",
                table: "MemoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MemoryItems_Conversations_ConversationId",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_Category",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_NormalizedKey",
                table: "MemoryItems");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewStatus",
                table: "MemoryItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "Accepted",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "MemoryItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "MemoryItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "DurableFact");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_CharacterId_ConversationId_Kind_NormalizedKey",
                table: "MemoryItems",
                columns: new[] { "CharacterId", "ConversationId", "Kind", "NormalizedKey" });

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_ConflictsWithMemoryItemId",
                table: "MemoryItems",
                column: "ConflictsWithMemoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_ExpiresAt",
                table: "MemoryItems",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_Kind",
                table: "MemoryItems",
                column: "Kind");

            migrationBuilder.AddForeignKey(
                name: "FK_MemoryItems_Characters_CharacterId",
                table: "MemoryItems",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemoryItems_Conversations_ConversationId",
                table: "MemoryItems",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemoryItems_MemoryItems_ConflictsWithMemoryItemId",
                table: "MemoryItems",
                column: "ConflictsWithMemoryItemId",
                principalTable: "MemoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemoryItems_Characters_CharacterId",
                table: "MemoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MemoryItems_Conversations_ConversationId",
                table: "MemoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MemoryItems_MemoryItems_ConflictsWithMemoryItemId",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_CharacterId_ConversationId_Kind_NormalizedKey",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_ConflictsWithMemoryItemId",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_ExpiresAt",
                table: "MemoryItems");

            migrationBuilder.DropIndex(
                name: "IX_MemoryItems_Kind",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "MemoryItems");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "MemoryItems");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewStatus",
                table: "MemoryItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "Accepted");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_Category",
                table: "MemoryItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_NormalizedKey",
                table: "MemoryItems",
                column: "NormalizedKey");

            migrationBuilder.AddForeignKey(
                name: "FK_MemoryItems_Characters_CharacterId",
                table: "MemoryItems",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemoryItems_Conversations_ConversationId",
                table: "MemoryItems",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

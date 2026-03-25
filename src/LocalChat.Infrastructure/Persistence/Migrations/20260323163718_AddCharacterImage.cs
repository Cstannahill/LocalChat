using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Characters",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImageUpdatedAt",
                table: "Characters",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_ImagePath",
                table: "Characters",
                column: "ImagePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Characters_ImagePath",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ImageUpdatedAt",
                table: "Characters");
        }
    }
}

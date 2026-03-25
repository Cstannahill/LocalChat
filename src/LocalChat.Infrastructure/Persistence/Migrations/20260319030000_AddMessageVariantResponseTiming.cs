using System;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260319030000_AddMessageVariantResponseTiming")]
    public partial class AddMessageVariantResponseTiming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GenerationCompletedAt",
                table: "MessageVariants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GenerationStartedAt",
                table: "MessageVariants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponseTimeMs",
                table: "MessageVariants",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GenerationCompletedAt",
                table: "MessageVariants");

            migrationBuilder.DropColumn(
                name: "GenerationStartedAt",
                table: "MessageVariants");

            migrationBuilder.DropColumn(
                name: "ResponseTimeMs",
                table: "MessageVariants");
        }
    }
}

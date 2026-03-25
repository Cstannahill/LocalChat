using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterVisualDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultVisualNegativePrompt",
                table: "Characters",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultVisualPromptPrefix",
                table: "Characters",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultVisualStylePreset",
                table: "Characters",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultVisualNegativePrompt",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "DefaultVisualPromptPrefix",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "DefaultVisualStylePreset",
                table: "Characters");
        }
    }
}

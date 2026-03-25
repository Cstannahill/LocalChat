using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MemoryAdminToolsAndUserProfileDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ReviewStatus",
                table: "MemoryItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "Accepted");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IsDefault",
                table: "UserProfiles",
                column: "IsDefault");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_IsDefault",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewStatus",
                table: "MemoryItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "Accepted",
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}

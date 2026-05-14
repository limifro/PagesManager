using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PagesManager.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddTextFormatting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBold",
                table: "Notes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsItalic",
                table: "Notes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnderline",
                table: "Notes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TextAlignment",
                table: "Notes",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBold",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "IsItalic",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "IsUnderline",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "TextAlignment",
                table: "Notes");
        }
    }
}

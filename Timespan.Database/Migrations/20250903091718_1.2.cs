using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timespan.Database.Migrations
{
    /// <inheritdoc />
    public partial class _12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "displayColorBlue",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "displayColorGreen",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "displayColorRed",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "running",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "displayColorBlue",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "displayColorGreen",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "displayColorRed",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "running",
                table: "Tasks");
        }
    }
}

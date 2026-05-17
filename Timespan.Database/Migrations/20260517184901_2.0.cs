using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timespan.Database.Migrations
{
    /// <inheritdoc />
    public partial class _20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Worker_ownerid",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ownerid",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ownerid",
                table: "Tickets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ownerid",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ownerid",
                table: "Tickets",
                column: "ownerid");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Worker_ownerid",
                table: "Tickets",
                column: "ownerid",
                principalTable: "Worker",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

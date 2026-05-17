using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timespan.Database.Migrations
{
    /// <inheritdoc />
    public partial class _10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Worker_ownerid",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Projects_projectName",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Worker_ownerid",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "projectName",
                table: "Tickets",
                newName: "ProjectName");

            migrationBuilder.RenameColumn(
                name: "ownerid",
                table: "Tickets",
                newName: "Ownerid");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Tickets",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Tickets",
                newName: "Description");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_projectName",
                table: "Tickets",
                newName: "IX_Tickets_ProjectName");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_ownerid",
                table: "Tickets",
                newName: "IX_Tickets_Ownerid");

            migrationBuilder.AlterColumn<long>(
                name: "ownerid",
                table: "Tasks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<long>(
                name: "ticketId",
                table: "Tasks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ticketId",
                table: "Tasks",
                column: "ticketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tickets_ticketId",
                table: "Tasks",
                column: "ticketId",
                principalTable: "Tickets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Worker_ownerid",
                table: "Tasks",
                column: "ownerid",
                principalTable: "Worker",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Projects_ProjectName",
                table: "Tickets",
                column: "ProjectName",
                principalTable: "Projects",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Worker_Ownerid",
                table: "Tickets",
                column: "Ownerid",
                principalTable: "Worker",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tickets_ticketId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Worker_ownerid",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Projects_ProjectName",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Worker_Ownerid",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ticketId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ticketId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "ProjectName",
                table: "Tickets",
                newName: "projectName");

            migrationBuilder.RenameColumn(
                name: "Ownerid",
                table: "Tickets",
                newName: "ownerid");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tickets",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Tickets",
                newName: "description");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_ProjectName",
                table: "Tickets",
                newName: "IX_Tickets_projectName");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_Ownerid",
                table: "Tickets",
                newName: "IX_Tickets_ownerid");

            migrationBuilder.AlterColumn<long>(
                name: "ownerid",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Worker_ownerid",
                table: "Tasks",
                column: "ownerid",
                principalTable: "Worker",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Projects_projectName",
                table: "Tickets",
                column: "projectName",
                principalTable: "Projects",
                principalColumn: "Name");

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

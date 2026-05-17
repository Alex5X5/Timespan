using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timespan.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Worker",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worker", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    ownerid = table.Column<long>(type: "INTEGER", nullable: false),
                    projectName = table.Column<string>(type: "TEXT", nullable: true),
                    start = table.Column<long>(type: "INTEGER", nullable: false),
                    finish = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_projectName",
                        column: x => x.projectName,
                        principalTable: "Projects",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_Tasks_Worker_ownerid",
                        column: x => x.ownerid,
                        principalTable: "Worker",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    ownerid = table.Column<long>(type: "INTEGER", nullable: false),
                    projectName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Projects_projectName",
                        column: x => x.projectName,
                        principalTable: "Projects",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_Tickets_Worker_ownerid",
                        column: x => x.ownerid,
                        principalTable: "Worker",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ownerid",
                table: "Tasks",
                column: "ownerid");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_projectName",
                table: "Tasks",
                column: "projectName");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ownerid",
                table: "Tickets",
                column: "ownerid");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_projectName",
                table: "Tickets",
                column: "projectName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Worker");
        }
    }
}

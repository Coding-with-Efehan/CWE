using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CWE.Data.Migrations
{
    public partial class Mutes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mutes",
                columns: table => new
                {
                    InfractionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    User = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    MuteStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MuteEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mutes");
        }
    }
}

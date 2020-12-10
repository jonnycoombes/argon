using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JCS.Argon.Migrations
{
    public partial class _102 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cache",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StringValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LongValue = table.Column<long>(type: "bigint", nullable: true),
                    IntValue = table.Column<int>(type: "int", nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cache", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cache",
                schema: "core");
        }
    }
}

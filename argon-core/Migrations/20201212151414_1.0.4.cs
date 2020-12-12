using Microsoft.EntityFrameworkCore.Migrations;

namespace JCS.Argon.Migrations
{
    public partial class _104 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Partition",
                schema: "core",
                table: "cache",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Partition",
                schema: "core",
                table: "cache");
        }
    }
}

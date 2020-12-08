using Microsoft.EntityFrameworkCore.Migrations;

namespace JCS.Argon.Migrations
{
    public partial class _100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageLocation",
                schema: "core",
                table: "version");

            migrationBuilder.DropColumn(
                name: "StorageLocation",
                schema: "core",
                table: "item");

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                schema: "core",
                table: "version",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Size",
                schema: "core",
                table: "version",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "StorageLocation",
                schema: "core",
                table: "version",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageLocation",
                schema: "core",
                table: "item",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

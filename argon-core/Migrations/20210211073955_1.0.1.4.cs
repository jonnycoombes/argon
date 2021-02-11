using Microsoft.EntityFrameworkCore.Migrations;

namespace JCS.Argon.Migrations
{
    public partial class _1014 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                "Size",
                schema: "argon",
                table: "collection",
                newName: "TotalSizeBytes");

            migrationBuilder.RenameColumn(
                "Length",
                schema: "argon",
                table: "collection",
                newName: "NumberOfItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                "TotalSizeBytes",
                schema: "argon",
                table: "collection",
                newName: "Size");

            migrationBuilder.RenameColumn(
                "NumberOfItems",
                schema: "argon",
                table: "collection",
                newName: "Length");
        }
    }
}
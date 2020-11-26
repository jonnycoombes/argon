using Microsoft.EntityFrameworkCore.Migrations;

namespace JCS.Argon.Migrations
{
    public partial class _01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowableValues",
                schema: "core",
                table: "constraint",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConstraintType",
                schema: "core",
                table: "constraint",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceProperty",
                schema: "core",
                table: "constraint",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetProperty",
                schema: "core",
                table: "constraint",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValueType",
                schema: "core",
                table: "constraint",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowableValues",
                schema: "core",
                table: "constraint");

            migrationBuilder.DropColumn(
                name: "ConstraintType",
                schema: "core",
                table: "constraint");

            migrationBuilder.DropColumn(
                name: "SourceProperty",
                schema: "core",
                table: "constraint");

            migrationBuilder.DropColumn(
                name: "TargetProperty",
                schema: "core",
                table: "constraint");

            migrationBuilder.DropColumn(
                name: "ValueType",
                schema: "core",
                table: "constraint");
        }
    }
}

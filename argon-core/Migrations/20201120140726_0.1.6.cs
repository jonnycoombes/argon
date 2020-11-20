using Microsoft.EntityFrameworkCore.Migrations;

namespace JCS.Argon.Migrations
{
    public partial class _016 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_item_PropertyGroupId",
                schema: "core",
                table: "item",
                column: "PropertyGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_item_propertyGroup_PropertyGroupId",
                schema: "core",
                table: "item",
                column: "PropertyGroupId",
                principalSchema: "core",
                principalTable: "propertyGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_item_propertyGroup_PropertyGroupId",
                schema: "core",
                table: "item");

            migrationBuilder.DropIndex(
                name: "IX_item_PropertyGroupId",
                schema: "core",
                table: "item");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JCS.Argon.Migrations
{
    public partial class _017 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PropertyGroupId",
                schema: "core",
                table: "collection",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_collection_PropertyGroupId",
                schema: "core",
                table: "collection",
                column: "PropertyGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_collection_propertyGroup_PropertyGroupId",
                schema: "core",
                table: "collection",
                column: "PropertyGroupId",
                principalSchema: "core",
                principalTable: "propertyGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_collection_propertyGroup_PropertyGroupId",
                schema: "core",
                table: "collection");

            migrationBuilder.DropIndex(
                name: "IX_collection_PropertyGroupId",
                schema: "core",
                table: "collection");

            migrationBuilder.DropColumn(
                name: "PropertyGroupId",
                schema: "core",
                table: "collection");
        }
    }
}

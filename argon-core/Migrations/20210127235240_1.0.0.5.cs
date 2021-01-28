using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JCS.Argon.Migrations
{
    public partial class _1005 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "argon");

            migrationBuilder.CreateTable(
                name: "cache",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Partition = table.Column<string>(type: "varchar(512)", nullable: false),
                    Key = table.Column<string>(type: "varchar(512)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StringValue = table.Column<string>(type: "varchar(512)", nullable: true),
                    LongValue = table.Column<long>(type: "bigint", nullable: true),
                    IntValue = table.Column<int>(type: "int", nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "constraintGroup",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_constraintGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "propertyGroup",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propertyGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "constraint",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>(type: "varchar(512)", nullable: false),
                    ConstraintType = table.Column<int>(type: "int", nullable: false),
                    SourceProperty = table.Column<string>(type: "varchar(512)", nullable: false),
                    TargetProperty = table.Column<string>(type: "varchar(512)", nullable: true),
                    ValueType = table.Column<int>(type: "int", nullable: true),
                    AllowableValues = table.Column<string>(type: "varchar(512)", nullable: true),
                    ConstraintGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_constraint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_constraint_constraintGroup_ConstraintGroupId",
                        column: x => x.ConstraintGroupId,
                        principalSchema: "argon",
                        principalTable: "constraintGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "collection",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>(type: "varchar(512)", nullable: false),
                    Description = table.Column<string>(type: "varchar(512)", nullable: true),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ProviderTag = table.Column<string>(type: "varchar(512)", nullable: false),
                    PropertyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConstraintGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_collection_constraintGroup_ConstraintGroupId",
                        column: x => x.ConstraintGroupId,
                        principalSchema: "argon",
                        principalTable: "constraintGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_collection_propertyGroup_PropertyGroupId",
                        column: x => x.PropertyGroupId,
                        principalSchema: "argon",
                        principalTable: "propertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "property",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>(type: "varchar(512)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StringValue = table.Column<string>(type: "varchar(512)", nullable: true),
                    NumberValue = table.Column<double>(type: "float", nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BooleanValue = table.Column<bool>(type: "bit", nullable: true),
                    PropertyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property", x => x.Id);
                    table.ForeignKey(
                        name: "FK_property_propertyGroup_PropertyGroupId",
                        column: x => x.PropertyGroupId,
                        principalSchema: "argon",
                        principalTable: "propertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>(type: "varchar(512)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_item_collection_CollectionId",
                        column: x => x.CollectionId,
                        principalSchema: "argon",
                        principalTable: "collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_propertyGroup_PropertyGroupId",
                        column: x => x.PropertyGroupId,
                        principalSchema: "argon",
                        principalTable: "propertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "itemVersion",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Major = table.Column<int>(type: "int", nullable: false),
                    Minor = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(512)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    MIMEType = table.Column<string>(type: "varchar(512)", nullable: true),
                    Thumbprint = table.Column<byte[]>(type: "varbinary(1024)", nullable: true),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itemVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_itemVersion_item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "argon",
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_collection_ConstraintGroupId",
                schema: "argon",
                table: "collection",
                column: "ConstraintGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_collection_PropertyGroupId",
                schema: "argon",
                table: "collection",
                column: "PropertyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_constraint_ConstraintGroupId",
                schema: "argon",
                table: "constraint",
                column: "ConstraintGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_item_CollectionId",
                schema: "argon",
                table: "item",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_item_PropertyGroupId",
                schema: "argon",
                table: "item",
                column: "PropertyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_itemVersion_ItemId",
                schema: "argon",
                table: "itemVersion",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_property_PropertyGroupId",
                schema: "argon",
                table: "property",
                column: "PropertyGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cache",
                schema: "argon");

            migrationBuilder.DropTable(
                name: "constraint",
                schema: "argon");

            migrationBuilder.DropTable(
                name: "itemVersion",
                schema: "argon");

            migrationBuilder.DropTable(
                name: "property",
                schema: "argon");

            migrationBuilder.DropTable(
                name: "item",
                schema: "argon");

            migrationBuilder.DropTable(
                name: "collection",
                schema: "argon");

            migrationBuilder.DropTable(
                name: "constraintGroup",
                schema: "argon");

            migrationBuilder.DropTable(
                name: "propertyGroup",
                schema: "argon");
        }
    }
}

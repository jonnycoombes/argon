using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JCS.Argon.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "constraintGroup",
                schema: "core",
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
                schema: "core",
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
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConstraintType = table.Column<int>(type: "int", nullable: false),
                    SourceProperty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetProperty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueType = table.Column<int>(type: "int", nullable: true),
                    AllowableValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConstraintGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_constraint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_constraint_constraintGroup_ConstraintGroupId",
                        column: x => x.ConstraintGroupId,
                        principalSchema: "core",
                        principalTable: "constraintGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "collection",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ProviderTag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConstraintGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_collection_constraintGroup_ConstraintGroupId",
                        column: x => x.ConstraintGroupId,
                        principalSchema: "core",
                        principalTable: "constraintGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_collection_propertyGroup_PropertyGroupId",
                        column: x => x.PropertyGroupId,
                        principalSchema: "core",
                        principalTable: "propertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "property",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StringValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                        principalSchema: "core",
                        principalTable: "propertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                        principalSchema: "core",
                        principalTable: "collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_propertyGroup_PropertyGroupId",
                        column: x => x.PropertyGroupId,
                        principalSchema: "core",
                        principalTable: "propertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "version",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Major = table.Column<int>(type: "int", nullable: false),
                    Minor = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    MIMEType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Thumbprint = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ProviderPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_version", x => x.Id);
                    table.ForeignKey(
                        name: "FK_version_item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "core",
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_collection_ConstraintGroupId",
                schema: "core",
                table: "collection",
                column: "ConstraintGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_collection_PropertyGroupId",
                schema: "core",
                table: "collection",
                column: "PropertyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_constraint_ConstraintGroupId",
                schema: "core",
                table: "constraint",
                column: "ConstraintGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_item_CollectionId",
                schema: "core",
                table: "item",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_item_PropertyGroupId",
                schema: "core",
                table: "item",
                column: "PropertyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_property_PropertyGroupId",
                schema: "core",
                table: "property",
                column: "PropertyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_version_ItemId",
                schema: "core",
                table: "version",
                column: "ItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "constraint",
                schema: "core");

            migrationBuilder.DropTable(
                name: "property",
                schema: "core");

            migrationBuilder.DropTable(
                name: "version",
                schema: "core");

            migrationBuilder.DropTable(
                name: "item",
                schema: "core");

            migrationBuilder.DropTable(
                name: "collection",
                schema: "core");

            migrationBuilder.DropTable(
                name: "constraintGroup",
                schema: "core");

            migrationBuilder.DropTable(
                name: "propertyGroup",
                schema: "core");
        }
    }
}

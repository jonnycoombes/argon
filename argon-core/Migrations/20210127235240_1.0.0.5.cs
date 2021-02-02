#region

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace JCS.Argon.Migrations
{
    public partial class _1005 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                "argon");

            migrationBuilder.CreateTable(
                "cache",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>("rowversion", rowVersion: true, nullable: true),
                    Partition = table.Column<string>("varchar(512)", nullable: false),
                    Key = table.Column<string>("varchar(512)", nullable: false),
                    Type = table.Column<int>("int", nullable: false),
                    StringValue = table.Column<string>("varchar(512)", nullable: true),
                    LongValue = table.Column<long>("bigint", nullable: true),
                    IntValue = table.Column<int>("int", nullable: true),
                    DateTimeValue = table.Column<DateTime>("datetime2", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_cache", x => x.Id); });

            migrationBuilder.CreateTable(
                "constraintGroup",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>("rowversion", rowVersion: true, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_constraintGroup", x => x.Id); });

            migrationBuilder.CreateTable(
                "propertyGroup",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>("rowversion", rowVersion: true, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_propertyGroup", x => x.Id); });

            migrationBuilder.CreateTable(
                "constraint",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>("rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>("varchar(512)", nullable: false),
                    ConstraintType = table.Column<int>("int", nullable: false),
                    SourceProperty = table.Column<string>("varchar(512)", nullable: false),
                    TargetProperty = table.Column<string>("varchar(512)", nullable: true),
                    ValueType = table.Column<int>("int", nullable: true),
                    AllowableValues = table.Column<string>("varchar(512)", nullable: true),
                    ConstraintGroupId = table.Column<Guid>("uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_constraint", x => x.Id);
                    table.ForeignKey(
                        "FK_constraint_constraintGroup_ConstraintGroupId",
                        x => x.ConstraintGroupId,
                        principalSchema: "argon",
                        principalTable: "constraintGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "collection",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>("rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>("varchar(512)", nullable: false),
                    Description = table.Column<string>("varchar(512)", nullable: true),
                    Length = table.Column<long>("bigint", nullable: false),
                    Size = table.Column<long>("bigint", nullable: false),
                    ProviderTag = table.Column<string>("varchar(512)", nullable: false),
                    PropertyGroupId = table.Column<Guid>("uniqueidentifier", nullable: true),
                    ConstraintGroupId = table.Column<Guid>("uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection", x => x.Id);
                    table.ForeignKey(
                        "FK_collection_constraintGroup_ConstraintGroupId",
                        x => x.ConstraintGroupId,
                        principalSchema: "argon",
                        principalTable: "constraintGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_collection_propertyGroup_PropertyGroupId",
                        x => x.PropertyGroupId,
                        principalSchema: "argon",
                        principalTable: "propertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "property",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>("rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>("varchar(512)", nullable: false),
                    Type = table.Column<int>("int", nullable: false),
                    StringValue = table.Column<string>("varchar(512)", nullable: true),
                    NumberValue = table.Column<double>("float", nullable: true),
                    DateTimeValue = table.Column<DateTime>("datetime2", nullable: true),
                    BooleanValue = table.Column<bool>("bit", nullable: true),
                    PropertyGroupId = table.Column<Guid>("uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property", x => x.Id);
                    table.ForeignKey(
                        "FK_property_propertyGroup_PropertyGroupId",
                        x => x.PropertyGroupId,
                        principalSchema: "argon",
                        principalTable: "propertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "item",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>("rowversion", rowVersion: true, nullable: true),
                    Name = table.Column<string>("varchar(512)", nullable: false),
                    CreatedDate = table.Column<DateTime>("datetime2", nullable: false),
                    LastModified = table.Column<DateTime>("datetime2", nullable: false),
                    CollectionId = table.Column<Guid>("uniqueidentifier", nullable: false),
                    PropertyGroupId = table.Column<Guid>("uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item", x => x.Id);
                    table.ForeignKey(
                        "FK_item_collection_CollectionId",
                        x => x.CollectionId,
                        principalSchema: "argon",
                        principalTable: "collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_item_propertyGroup_PropertyGroupId",
                        x => x.PropertyGroupId,
                        principalSchema: "argon",
                        principalTable: "propertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "itemVersion",
                schema: "argon",
                columns: table => new
                {
                    Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>("rowversion", rowVersion: true, nullable: true),
                    Major = table.Column<int>("int", nullable: false),
                    Minor = table.Column<int>("int", nullable: false),
                    Name = table.Column<string>("varchar(512)", nullable: false),
                    Size = table.Column<long>("bigint", nullable: false),
                    MIMEType = table.Column<string>("varchar(512)", nullable: true),
                    Thumbprint = table.Column<byte[]>("varbinary(1024)", nullable: true),
                    ItemId = table.Column<Guid>("uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itemVersion", x => x.Id);
                    table.ForeignKey(
                        "FK_itemVersion_item_ItemId",
                        x => x.ItemId,
                        principalSchema: "argon",
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_collection_ConstraintGroupId",
                schema: "argon",
                table: "collection",
                column: "ConstraintGroupId");

            migrationBuilder.CreateIndex(
                "IX_collection_PropertyGroupId",
                schema: "argon",
                table: "collection",
                column: "PropertyGroupId");

            migrationBuilder.CreateIndex(
                "IX_constraint_ConstraintGroupId",
                schema: "argon",
                table: "constraint",
                column: "ConstraintGroupId");

            migrationBuilder.CreateIndex(
                "IX_item_CollectionId",
                schema: "argon",
                table: "item",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                "IX_item_PropertyGroupId",
                schema: "argon",
                table: "item",
                column: "PropertyGroupId");

            migrationBuilder.CreateIndex(
                "IX_itemVersion_ItemId",
                schema: "argon",
                table: "itemVersion",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                "IX_property_PropertyGroupId",
                schema: "argon",
                table: "property",
                column: "PropertyGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "cache",
                "argon");

            migrationBuilder.DropTable(
                "constraint",
                "argon");

            migrationBuilder.DropTable(
                "itemVersion",
                "argon");

            migrationBuilder.DropTable(
                "property",
                "argon");

            migrationBuilder.DropTable(
                "item",
                "argon");

            migrationBuilder.DropTable(
                "collection",
                "argon");

            migrationBuilder.DropTable(
                "constraintGroup",
                "argon");

            migrationBuilder.DropTable(
                "propertyGroup",
                "argon");
        }
    }
}
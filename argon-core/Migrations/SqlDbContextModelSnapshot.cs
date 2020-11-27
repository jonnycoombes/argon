﻿// <auto-generated />
using System;
using JCS.Argon.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JCS.Argon.Migrations
{
    [DbContext(typeof(SqlDbContext))]
    partial class SqlDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("JCS.Argon.Model.Schema.Collection", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ConstraintGroupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Length")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("PropertyGroupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProviderTag")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("ConstraintGroupId");

                    b.HasIndex("PropertyGroupId");

                    b.ToTable("collection", "core");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Constraint", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AllowableValues")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ConstraintGroupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ConstraintType")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SourceProperty")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TargetProperty")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int?>("ValueType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ConstraintGroupId");

                    b.ToTable("constraint", "core");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.ConstraintGroup", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.ToTable("constraintGroup", "core");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Item", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CollectionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("PropertyGroupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.HasIndex("PropertyGroupId");

                    b.ToTable("item", "core");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Property", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool?>("BooleanValue")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("DateTimeValue")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("NumberValue")
                        .HasColumnType("float");

                    b.Property<Guid>("PropertyGroupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("StringValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PropertyGroupId");

                    b.ToTable("property", "core");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.PropertyGroup", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.ToTable("propertyGroup", "core");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Version", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ItemId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("MIMEType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Major")
                        .HasColumnType("int");

                    b.Property<int>("Minor")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProviderPath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Size")
                        .HasColumnType("int");

                    b.Property<byte[]>("Thumbprint")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("ItemId");

                    b.ToTable("version", "core");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Collection", b =>
                {
                    b.HasOne("JCS.Argon.Model.Schema.ConstraintGroup", "ConstraintGroup")
                        .WithMany()
                        .HasForeignKey("ConstraintGroupId");

                    b.HasOne("JCS.Argon.Model.Schema.PropertyGroup", "PropertyGroup")
                        .WithMany()
                        .HasForeignKey("PropertyGroupId");

                    b.Navigation("ConstraintGroup");

                    b.Navigation("PropertyGroup");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Constraint", b =>
                {
                    b.HasOne("JCS.Argon.Model.Schema.ConstraintGroup", null)
                        .WithMany("Constraints")
                        .HasForeignKey("ConstraintGroupId");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Item", b =>
                {
                    b.HasOne("JCS.Argon.Model.Schema.Collection", "Collection")
                        .WithMany("Items")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("JCS.Argon.Model.Schema.PropertyGroup", "PropertyGroup")
                        .WithMany()
                        .HasForeignKey("PropertyGroupId");

                    b.Navigation("Collection");

                    b.Navigation("PropertyGroup");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Property", b =>
                {
                    b.HasOne("JCS.Argon.Model.Schema.PropertyGroup", "PropertyGroup")
                        .WithMany("Properties")
                        .HasForeignKey("PropertyGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PropertyGroup");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Version", b =>
                {
                    b.HasOne("JCS.Argon.Model.Schema.Item", "Item")
                        .WithMany("Versions")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Item");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Collection", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.ConstraintGroup", b =>
                {
                    b.Navigation("Constraints");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Item", b =>
                {
                    b.Navigation("Versions");
                });

            modelBuilder.Entity("JCS.Argon.Model.Schema.PropertyGroup", b =>
                {
                    b.Navigation("Properties");
                });
#pragma warning restore 612, 618
        }
    }
}

﻿#region

using System;
using JCS.Argon.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#endregion

namespace JCS.Argon.Migrations
{
    [DbContext(typeof(SqlDbContext))]
    internal class SqlDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            #pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("JCS.Argon.Model.Schema.CacheEntry", b =>
            {
                b.Property<Guid?>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime?>("DateTimeValue")
                    .HasColumnType("datetime2");

                b.Property<int?>("IntValue")
                    .HasColumnType("int");

                b.Property<string>("Key")
                    .IsRequired()
                    .HasColumnType("varchar(512)");

                b.Property<long?>("LongValue")
                    .HasColumnType("bigint");

                b.Property<string>("Partition")
                    .IsRequired()
                    .HasColumnType("varchar(512)");

                b.Property<string>("StringValue")
                    .HasColumnType("varchar(512)");

                b.Property<byte[]>("Timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("rowversion");

                b.Property<int>("Type")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.ToTable("cache", "argon");
            });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Collection", b =>
            {
                b.Property<Guid?>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<Guid?>("ConstraintGroupId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Description")
                    .HasColumnType("varchar(512)");

                b.Property<long>("Length")
                    .HasColumnType("bigint");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("varchar(512)");

                b.Property<Guid?>("PropertyGroupId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("ProviderTag")
                    .IsRequired()
                    .HasColumnType("varchar(512)");

                b.Property<long>("Size")
                    .HasColumnType("bigint");

                b.Property<byte[]>("Timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("rowversion");

                b.HasKey("Id");

                b.HasIndex("ConstraintGroupId");

                b.HasIndex("PropertyGroupId");

                b.ToTable("collection", "argon");
            });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Constraint", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("AllowableValues")
                    .HasColumnType("varchar(512)");

                b.Property<Guid?>("ConstraintGroupId")
                    .HasColumnType("uniqueidentifier");

                b.Property<int>("ConstraintType")
                    .HasColumnType("int");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("varchar(512)");

                b.Property<string>("SourceProperty")
                    .IsRequired()
                    .HasColumnType("varchar(512)");

                b.Property<string>("TargetProperty")
                    .HasColumnType("varchar(512)");

                b.Property<byte[]>("Timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("rowversion");

                b.Property<int?>("ValueType")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("ConstraintGroupId");

                b.ToTable("constraint", "argon");
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

                b.ToTable("constraintGroup", "argon");
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
                    .HasColumnType("varchar(512)");

                b.Property<Guid?>("PropertyGroupId")
                    .HasColumnType("uniqueidentifier");

                b.Property<byte[]>("Timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("rowversion");

                b.HasKey("Id");

                b.HasIndex("CollectionId");

                b.HasIndex("PropertyGroupId");

                b.ToTable("item", "argon");
            });

            modelBuilder.Entity("JCS.Argon.Model.Schema.ItemVersion", b =>
            {
                b.Property<Guid?>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<Guid>("ItemId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("MIMEType")
                    .HasColumnType("varchar(512)");

                b.Property<int>("Major")
                    .HasColumnType("int");

                b.Property<int>("Minor")
                    .HasColumnType("int");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("varchar(512)");

                b.Property<long>("Size")
                    .HasColumnType("bigint");

                b.Property<byte[]>("Thumbprint")
                    .HasColumnType("varbinary(1024)");

                b.Property<byte[]>("Timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("rowversion");

                b.HasKey("Id");

                b.HasIndex("ItemId");

                b.ToTable("itemVersion", "argon");
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
                    .HasColumnType("varchar(512)");

                b.Property<double?>("NumberValue")
                    .HasColumnType("float");

                b.Property<Guid>("PropertyGroupId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("StringValue")
                    .HasColumnType("varchar(512)");

                b.Property<byte[]>("Timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("rowversion");

                b.Property<int>("Type")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("PropertyGroupId");

                b.ToTable("property", "argon");
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

                b.ToTable("propertyGroup", "argon");
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

            modelBuilder.Entity("JCS.Argon.Model.Schema.ItemVersion", b =>
            {
                b.HasOne("JCS.Argon.Model.Schema.Item", "Item")
                    .WithMany("Versions")
                    .HasForeignKey("ItemId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Item");
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

            modelBuilder.Entity("JCS.Argon.Model.Schema.Collection", b => { b.Navigation("Items"); });

            modelBuilder.Entity("JCS.Argon.Model.Schema.ConstraintGroup", b => { b.Navigation("Constraints"); });

            modelBuilder.Entity("JCS.Argon.Model.Schema.Item", b => { b.Navigation("Versions"); });

            modelBuilder.Entity("JCS.Argon.Model.Schema.PropertyGroup", b => { b.Navigation("Properties"); });
            #pragma warning restore 612, 618
        }
    }
}
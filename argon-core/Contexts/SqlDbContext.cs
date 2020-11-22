using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JCS.Argon.Model.Schema;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace JCS.Argon.Contexts
{
    /// <summary>
    /// The main database context used throughout the core
    /// </summary>
    public class SqlDbContext : DbContext
    {
        /// <summary>
        /// The set of all <see cref="Collection"/> model elements
        /// </summary>
        public DbSet<Collection> Collections { get; set; } = null!;

        /// <summary>
        /// The set of all <see cref="Item"/> model elements
        /// </summary>
        public DbSet<Item> Items { get; set; } = null!;

        /// <summary>
        /// The set of all <see cref="Version"/> model elements
        /// </summary>
        public DbSet<Version> Versions { get; set; } = null!;

        /// <summary>
        /// The set of all <see cref="PropertyGroup"/> model elements
        /// </summary>
        public DbSet<PropertyGroup> PropertyGroups { get; set; } = null!;

        /// <summary>
        /// The set of all <see cref="Property"/> model elements
        /// </summary>
        public DbSet<Property> Properties { get; set; } = null!;

        public SqlDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
            
        }

        /// <summary>
        /// Perform any more technical modifications to the schema creation logic in here
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Log.ForContext("SourceContext", "SqlDbContext")
                .Information("onModelCreating called performing any additional tasks");
            
            // setup the linkage between collections and their items
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Collection)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CollectionId);
            
            // linkage between items and their versions - probably doesn't need to be done explicitly
            modelBuilder.Entity<Item>()
                .HasMany(i => i.Versions);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
#region

using System;
using System.Diagnostics.CodeAnalysis;
using JCS.Argon.Model.Schema;
using Microsoft.EntityFrameworkCore;
using Serilog;

#endregion

namespace JCS.Argon.Contexts
{
    /// <summary>
    ///     The main database context used throughout the core
    /// </summary>
    public class SqlDbContext : DbContext
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="options"></param>
        public SqlDbContext([NotNullAttribute]
            DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        ///     The set of all <see cref="Collection" /> model elements
        /// </summary>
        public DbSet<Collection> Collections { get; set; } = null!;

        /// <summary>
        ///     The set of all <see cref="Item" /> model elements
        /// </summary>
        public DbSet<Item> Items { get; set; } = null!;

        /// <summary>
        ///     The set of all <see cref="ItemVersion" /> model elements
        /// </summary>
        public DbSet<ItemVersion> Versions { get; set; } = null!;

        /// <summary>
        ///     The set of all <see cref="PropertyGroup" /> model elements
        /// </summary>
        public DbSet<PropertyGroup> PropertyGroups { get; set; } = null!;

        /// <summary>
        ///     The set of all <see cref="Property" /> model elements
        /// </summary>
        public DbSet<Property> Properties { get; set; } = null!;

        /// <summary>
        ///     The set of all <see cref="ConstraintGroup" /> model elements
        /// </summary>
        public DbSet<ConstraintGroup> ConstraintGroups { get; set; } = null!;

        /// <summary>
        ///     The set of all <see cref="Constraint" /> model elements
        /// </summary>
        public DbSet<Constraint> Constraints { get; set; } = null!;

        /// <summary>
        ///     The set of all <see cref="CacheEntry" /> model elements
        /// </summary>
        public DbSet<CacheEntry> CacheEntries { get; set; } = null!;

        /// <summary>
        ///     Perform any more technical modifications to the schema creation logic in here
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Log.ForContext("SourceContext", "SqlDbContext")
                .Information("onModelCreating called performing any additional tasks");

            // add a custom conversion for string arrays 
            modelBuilder.Entity<Constraint>()
                .Property(c => c.AllowableValues)
                .HasConversion(v => string.Join(',', v ?? Array.Empty<string>()),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

            base.OnModelCreating(modelBuilder);
        }
    }
}
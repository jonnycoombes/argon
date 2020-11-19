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

        public DbSet<Dictionary<string, object>> Properties => Set<Dictionary<string, object>>("Category");

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
            
            modelBuilder.SharedTypeEntity<Dictionary<string, object>>("Category", b =>
            {
                b.IndexerProperty<string>("Description");
                b.IndexerProperty<int>("Id");
                b.IndexerProperty<string>("Name").IsRequired();
            });
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
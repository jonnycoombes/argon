using System.Diagnostics.CodeAnalysis;
using JCS.Argon.Model.Schema;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<Collection> Collections { get; set; }
        
        public SqlDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
            
        }
    }
}
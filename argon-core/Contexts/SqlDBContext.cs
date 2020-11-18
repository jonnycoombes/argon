using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace JCS.Argon.Contexts
{
    /// <summary>
    /// The main database context used throughout the core
    /// </summary>
    public class SqlDBContext : DbContext
    {
        public SqlDBContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
            
        }
    }
}
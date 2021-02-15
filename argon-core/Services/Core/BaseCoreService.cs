#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.VSP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Base class for services within Argon.  Uses the <see cref="IServiceProvider" /> resolution "anti-pattern" for the
    ///     resolution of
    ///     service properties further down the heirarchy.  This keeps hard dependencies to a minimum and also allows for
    ///     remote/proxy injection
    ///     of services at a later date if required.  Also, through the service properties exposed - it is possible to reflect
    ///     on the potential
    ///     set of dependencies within a derived class.
    /// </summary>
    public abstract class BaseCoreService
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<BaseCoreService>();

        /// <summary>
        ///     The current system configuration
        /// </summary>
        private readonly IOptionsMonitor<ApiOptions> _options;

        /// <summary>
        ///     The DI-injected service provider
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     The current scoped <see cref="IConstraintGroupManager" /> instance
        /// </summary>
        private IConstraintGroupManager? _constraintGroupManager;

        /// <summary>
        ///     An underlying DB context
        /// </summary>
        private SqlDbContext? _dbContext;

        /// <summary>
        ///     The currently scoped <see cref="IItemManager" /> instance
        /// </summary>
        private IItemManager? _itemManager;

        /// <summary>
        ///     The currently scoped <see cref="IPropertyGroupManager" /> instance
        /// </summary>
        private IPropertyGroupManager? _propertyGroupManager;

        /// <summary>
        ///     The currently configured <see cref="IVirtualStorageManager" /> instance
        /// </summary>
        private IVirtualStorageManager? _virtualStorageManager;

        /// <summary>
        ///     Default constructor - just takes current system configuration along with
        ///     the DI <see cref="IServiceProvider" />.  (Reduces the explicit injected constructor params in sub-classes)
        /// </summary>
        /// <param name="options">The current system configuration</param>
        /// <param name="serviceProvider">An active <see cref="IServiceProvider" /> instance</param>
        protected BaseCoreService(IOptionsMonitor<ApiOptions> options, IServiceProvider serviceProvider)
        {
            LogMethodCall(_log);
            _serviceProvider = serviceProvider;
            _options = options;
        }

        /// <summary>
        ///     Access through resolution
        /// </summary>
        protected SqlDbContext DbContext => ResolveDbContext();

        /// <summary>
        ///     Access through resolution
        /// </summary>
        protected IConstraintGroupManager ConstraintGroupManager => ResolveConstraintGroupManager();

        /// <summary>
        ///     Access through resolution
        /// </summary>
        protected IItemManager ItemManager => ResolveItemManager();

        /// <summary>
        ///     Access through resolution
        /// </summary>
        protected IPropertyGroupManager PropertyGroupManager => ResolvePropertyGroupManager();

        /// <summary>
        ///     Access through resolution
        /// </summary>
        protected IVirtualStorageManager VirtualStorageManager => ResolveVirtualStorageManager();

        /// <summary>
        ///     Access without the <see cref="IOptionsMonitor{TOptions}" /> wrapping
        /// </summary>
        protected ApiOptions Options => _options.CurrentValue;

        /// <summary>
        ///     Access through resolution - a different <see cref="IServiceProvider" /> could be
        ///     supplanted if required
        /// </summary>
        protected IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        ///     Accessor for the current <see cref="SqlDbContext" /> instance
        /// </summary>
        /// <returns></returns>
        private SqlDbContext ResolveDbContext()
        {
            LogMethodCall(_log);
            return _dbContext ??= _serviceProvider.GetRequiredService<SqlDbContext>();
        }

        /// <summary>
        ///     Accessor for the current <see cref="IConstraintGroupManager" /> instance
        /// </summary>
        /// <returns></returns>
        private IConstraintGroupManager ResolveConstraintGroupManager()
        {
            LogMethodCall(_log);
            return _constraintGroupManager ??= _serviceProvider.GetRequiredService<IConstraintGroupManager>();
        }

        /// <summary>
        ///     Accessor for the current <see cref="IItemManager" /> instance
        /// </summary>
        /// <returns></returns>
        private IItemManager ResolveItemManager()
        {
            LogMethodCall(_log);
            return _itemManager ??= _serviceProvider.GetRequiredService<IItemManager>();
        }

        /// <summary>
        ///     Accessor for the current <see cref="IPropertyGroupManager" /> instance
        /// </summary>
        /// <returns></returns>
        private IPropertyGroupManager ResolvePropertyGroupManager()
        {
            LogMethodCall(_log);
            return _propertyGroupManager ??= _serviceProvider.GetRequiredService<IPropertyGroupManager>();
        }

        /// <summary>
        ///     Accessor for the current <see cref="IVirtualStorageManager" /> instance
        /// </summary>
        /// <returns></returns>
        private IVirtualStorageManager ResolveVirtualStorageManager()
        {
            LogMethodCall(_log);
            return _virtualStorageManager ??= _serviceProvider.GetRequiredService<IVirtualStorageManager>();
        }

        /// <summary>
        ///     Method which makes a call to save changes to the underlying db context, and traps any concurrency exceptions.
        /// </summary>
        protected async Task CheckedContextSave()
        {
            LogMethodCall(_log);
            var saved = false;
            while (!saved)
            {
                try
                {
                    await DbContext.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        // check for a concurrency token and check its type
                        if (entry.Properties.Any(p => p.Metadata.IsConcurrencyToken))
                        {
                            var tokenProperty = entry.Properties.Where(p => p.Metadata.IsConcurrencyToken).First();
                            if (tokenProperty.Metadata.ClrType == typeof(byte[]))
                            {
                                var databaseValues = entry.GetDatabaseValues();
                                var currentTimestamp = ConvertTimestamp((byte[]) entry.CurrentValues[tokenProperty.Metadata.Name]);
                                var databaseTimestamp = ConvertTimestamp((byte[]) databaseValues[tokenProperty.Metadata.Name]);
                                LogVerbose(_log, $"Concurrent conflict: (local: {currentTimestamp}, remote: {databaseTimestamp})");

                                // currently cached copy is behind the database, so update with database values and attempt a 
                                // re-save
                                if (currentTimestamp < databaseTimestamp)
                                {
                                    entry.CurrentValues.SetValues(databaseValues);
                                }

                                // in-sync with or ahead of the underlying database - so completed
                                if (currentTimestamp >= databaseTimestamp)
                                {
                                    saved = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Converts a ROWVERSION byte-array into a long value
        /// </summary>
        /// <param name="raw">The raw timestamp value (assumed to be equivalent to a SQL ROWVERSION stored in big endian)</param>
        /// <returns>A <see cref="long" /> representing the timestamp</returns>
        private long ConvertTimestamp(byte[] raw)
        {
            Debug.Assert(raw.Length == 8);
            return BitConverter.ToInt64(raw.Reverse().ToArray(), 0);
        }
    }
}
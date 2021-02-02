#region

using System;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.VSP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            return _dbContext ??= _serviceProvider.GetRequiredService<SqlDbContext>();
        }

        /// <summary>
        ///     Accessor for the current <see cref="IConstraintGroupManager" /> instance
        /// </summary>
        /// <returns></returns>
        private IConstraintGroupManager ResolveConstraintGroupManager()
        {
            return _constraintGroupManager ??= _serviceProvider.GetRequiredService<IConstraintGroupManager>();
        }

        /// <summary>
        ///     Accessor for the current <see cref="IItemManager" /> instance
        /// </summary>
        /// <returns></returns>
        private IItemManager ResolveItemManager()
        {
            return _itemManager ??= _serviceProvider.GetRequiredService<IItemManager>();
        }

        /// <summary>
        ///     Accessor for the current <see cref="IPropertyGroupManager" /> instance
        /// </summary>
        /// <returns></returns>
        private IPropertyGroupManager ResolvePropertyGroupManager()
        {
            return _propertyGroupManager ??= _serviceProvider.GetRequiredService<IPropertyGroupManager>();
        }

        /// <summary>
        ///     Accessor for the current <see cref="IVirtualStorageManager" /> instance
        /// </summary>
        /// <returns></returns>
        private IVirtualStorageManager ResolveVirtualStorageManager()
        {
            return _virtualStorageManager ??= _serviceProvider.GetRequiredService<IVirtualStorageManager>();
        }
    }
}
using System;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.VSP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JCS.Argon.Services.Core
{
    public abstract class BaseCoreService
    {
        /// <summary>
        /// An underlying DB context
        /// </summary>
        private SqlDbContext _dbContext;

        /// <summary>
        /// Access through resolution
        /// </summary>
        protected SqlDbContext DbContext => ResolveDbContext();
        
        /// <summary>
        /// The current scoped <see cref="IConstraintGroupManager"/> instance
        /// </summary>
        private IConstraintGroupManager _constraintGroupManager;

        /// <summary>
        /// Access through resolution
        /// </summary>
        protected IConstraintGroupManager ConstraintGroupManager => ResolveConstraintGroupManager();

        /// <summary>
        /// The currently scoped <see cref="IItemManager"/> instance
        /// </summary>
        private  IItemManager _itemManager;

        /// <summary>
        /// Access through resolution
        /// </summary>
        protected IItemManager ItemManager => ResolveItemManager();
        
        /// <summary>
        /// The currently scoped <see cref="IPropertyGroupManager"/> instance
        /// </summary>
        private  IPropertyGroupManager _propertyGroupManager;

        /// <summary>
        /// Access through resolution
        /// </summary>
        protected IPropertyGroupManager PropertyGroupManager => ResolvePropertyGroupManager();
        
        /// <summary>
        /// The currently configured <see cref="IVirtualStorageManager"/> instance
        /// </summary>
        private IVirtualStorageManager _virtualStorageManager;

        /// <summary>
        /// Access through resolution
        /// </summary>
        protected IVirtualStorageManager VirtualStorageManager => ResolveVirtualStorageManager();
        
        /// <summary>
        /// The current system configuration
        /// </summary>
        private IOptionsMonitor<ApiOptions> _options;

        /// <summary>
        /// Access without the <see cref="IOptionsMonitor{TOptions}"/> wrapping
        /// </summary>
        protected ApiOptions Options => _options.CurrentValue;

        /// <summary>
        /// The DI-injected service provider
        /// </summary>
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Access through resolution - a different <see cref="IServiceProvider"/> could be
        /// supplanted if required
        /// </summary>
        protected IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// Default constructor - just takes current system configuration along with
        /// the DI <see cref="IServiceProvider"/>.  (Reduces the explicit injected constructor params in sub-classes)
        /// </summary>
        /// <param name="options">The current system configuration</param>
        /// <param name="serviceProvider">An active <see cref="IServiceProvider"/> instance</param>
        protected BaseCoreService(IOptionsMonitor<ApiOptions> options, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        /// <summary>
        /// Accessor for the current <see cref="SqlDbContext"/> instance
        /// </summary>
        /// <returns></returns>
        private SqlDbContext ResolveDbContext()
        {
            if (_dbContext == null)
            {
                _dbContext= _serviceProvider.GetRequiredService<SqlDbContext>();
            }
            return _dbContext;
        }

        /// <summary>
        /// Accessor for the current <see cref="IConstraintGroupManager"/> instance 
        /// </summary>
        /// <returns></returns>
        private IConstraintGroupManager ResolveConstraintGroupManager()
        {
            if (_constraintGroupManager == null)
            {
                _constraintGroupManager= _serviceProvider.GetRequiredService<IConstraintGroupManager>();
            }
            return _constraintGroupManager;
        }

        /// <summary>
        /// Accessor for the current <see cref="IItemManager"/> instance
        /// </summary>
        /// <returns></returns>
        private IItemManager ResolveItemManager()
        {
            if (_itemManager == null)
            {
                _itemManager= _serviceProvider.GetRequiredService<IItemManager>();
            }
            return _itemManager;
        }

        /// <summary>
        /// Accessor for the current <see cref="IPropertyGroupManager"/> instance
        /// </summary>
        /// <returns></returns>
        private IPropertyGroupManager ResolvePropertyGroupManager()
        {
            if (_propertyGroupManager == null)
            {
                _propertyGroupManager= _serviceProvider.GetRequiredService<IPropertyGroupManager>();
            }
            return _propertyGroupManager;
        }

        /// <summary>
        /// Accessor for the current <see cref="IVirtualStorageManager"/> instance
        /// </summary>
        /// <returns></returns>
        private IVirtualStorageManager ResolveVirtualStorageManager()
        {
            if (_virtualStorageManager == null)
            {
               _virtualStorageManager=  _serviceProvider.GetRequiredService<IVirtualStorageManager>();
            }
            return _virtualStorageManager;
        }
    }
}
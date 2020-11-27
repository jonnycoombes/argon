using System;
using System.Collections.Generic;
using System.Linq;
using JCS.Argon.Contexts;
using JCS.Argon.Helpers;
using JCS.Argon.Model.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JCS.Argon.Services.VSP
{
    /// <summary>
    /// Default implementation of the VSP registry
    /// </summary>
    public class VirtualStorageManager  : IVirtualStorageManager
    {
        /// <summary>
        /// The overall application configuration, used to extract VSP bindings
        /// </summary>
        private readonly VirtualStorageConfiguration _virtualStorageConfiguration;

        /// <summary>
        /// Logger for logging
        /// </summary>
        private readonly ILogger<VirtualStorageManager> _log;

        public VirtualStorageManager(ILogger<VirtualStorageManager> log, IOptionsMonitor<VirtualStorageConfiguration> vspConfiguration)
        {
            log.LogDebug("Creating new instance");
            _virtualStorageConfiguration= vspConfiguration.CurrentValue;
            _log = log;
            ResolveProviders();
            BindProviders();
        }

        protected void BindProviders()
        {
            
        }
        
        /// <summary>
        /// Scans the current runtime environment and looks for types that implement the <see cref="IVirtualStorageProvider"/>
        /// interface
        /// </summary>
        /// <exception cref="IVirtualStorageManager.VspFactoryAwareException"></exception>
        protected void ResolveProviders()
        {
            _log.LogInformation("Resolving providers within the current runtime environment");
            try
            {
                var providerTypes = ReflectionHelper.LocateAllImplementors<IVirtualStorageProvider>()
                    .Where(t => !t.IsAbstract && !t.IsInterface);
                foreach (var providerType in providerTypes)
                {
                    var instance = (IVirtualStorageProvider)ReflectionHelper.InstantiateType(providerType, _log);
                    _log.LogInformation($"Found VSP provider implementation: ({providerType.FullName},{instance.ProviderType})");
                }
            }
            catch (Exception ex)
            {
                throw new IVirtualStorageManager.VspFactoryAwareException(500, "Failed to load VSP providers", ex);
            }
        }
        
        public List<VirtualStorageBinding> GetBindings()
        {
            return _virtualStorageConfiguration.Bindings;
        }

        public IVirtualStorageProvider GetProvider(string tag)
        {
            _log.LogDebug($"Attempting instantiation of VSP provider with tag [{tag}]");
            throw new System.NotImplementedException();
        }
    }
}
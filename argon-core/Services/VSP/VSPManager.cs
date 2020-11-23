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
    public class VSPManager  : IVSPManager
    {
        /// <summary>
        /// The overall application configuration, used to extract VSP bindings
        /// </summary>
        private readonly VSPConfiguration _vspConfiguration;

        /// <summary>
        /// Logger for logging
        /// </summary>
        private readonly ILogger<VSPManager> _log;

        public VSPManager(ILogger<VSPManager> log, IOptionsMonitor<VSPConfiguration> vspConfiguration)
        {
            log.LogDebug("Creating new instance");
            _vspConfiguration= vspConfiguration.CurrentValue;
            _log = log;
            ResolveProviders();
            BindProviders();
        }

        protected void BindProviders()
        {
            
        }
        
        /// <summary>
        /// Scans the current runtime environment and looks for types that implement the <see cref="IVSPProvider"/>
        /// interface
        /// </summary>
        /// <exception cref="IVSPManager.VspFactoryAwareException"></exception>
        protected void ResolveProviders()
        {
            _log.LogInformation("Resolving providers within the current runtime environment");
            try
            {
                var providerTypes = ReflectionHelper.LocateAllImplementors<IVSPProvider>()
                    .Where(t => !t.IsAbstract && !t.IsInterface);
                foreach (var providerType in providerTypes)
                {
                    var instance = (IVSPProvider)ReflectionHelper.InstantiateType(providerType, _log);
                    _log.LogInformation($"Found VSP provider implementation: ({providerType.FullName},{instance.ProviderType})");
                }
            }
            catch (Exception ex)
            {
                throw new IVSPManager.VspFactoryAwareException(500, "Failed to load VSP providers", ex);
            }
        }
        
        public List<VSPBinding> GetBindings()
        {
            return _vspConfiguration.Bindings;
        }

        public IVSPProvider GetProvider(string tag)
        {
            _log.LogDebug($"Attempting instantiation of VSP provider with tag [{tag}]");
            throw new System.NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using JCS.Argon.Contexts;
using JCS.Argon.Helpers;
using JCS.Argon.Model.Configuration;
using Microsoft.AspNetCore.Http;
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
        /// Internal cache of the types associated with different <see cref="IVirtualStorageProvider"/> implementations
        /// </summary>
        private readonly Dictionary<string, Type> _providerTypesMap = new Dictionary<string, Type>();

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
        }

        /// <summary>
        /// Check whether a given provider type exists
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        protected bool ProviderExists(string providerType)
        {
            return _providerTypesMap.ContainsKey(providerType);
        }

        /// <summary>
        /// Checks whether or not we have a provider binding for a specified tag
        /// </summary>
        /// <param name="tag">The tag for the provider</param>
        /// <returns></returns>
        protected bool BindingExistsForTag(string tag)
        {
            return _virtualStorageConfiguration.Bindings.Any(b => b.Tag == tag);
        }

        /// <summary>
        /// Create an instance of a <see cref="IVirtualStorageProvider"/> based on the short
        /// providerType value
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        /// <exception cref="IVirtualStorageManager.VirtualStorageManagerException"></exception>
        protected IVirtualStorageProvider CreateProviderInstance(string providerType)
        {
            _log.LogDebug($"Instantiating a virtual storage provider of type [{providerType}]");
            if (ProviderExists(providerType))
            {
#pragma warning disable 8600
                IVirtualStorageProvider instance =(IVirtualStorageProvider)ReflectionHelper.InstantiateType(_providerTypesMap[providerType], _log); 
#pragma warning restore 8600
                if (instance == null)
                {
                    throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                        $"Failed to instance a new instance of a virtual storage provider with type: [{providerType}]"); 
                }
                return instance;
            }
            else
            {
                throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                    $"Request for a virtual storage provider which doesn't appear to exist: [{providerType}]");
            }
        }

        
        /// <summary>
        /// Scans the current runtime environment and looks for types that implement the <see cref="IVirtualStorageProvider"/>
        /// interface
        /// </summary>
        /// <exception cref="IVirtualStorageManager.VirtualStorageManagerException"></exception>
        protected void ResolveProviders()
        {
            _log.LogInformation("Resolving providers within the current runtime environment");
            try
            {
                var providerTypes = ReflectionHelper.LocateAllImplementors<IVirtualStorageProvider>()
                    .Where(t => !t.IsAbstract && !t.IsInterface);
                foreach (var providerType in providerTypes)
                {
#pragma warning disable 8600
                    var instance = (IVirtualStorageProvider)ReflectionHelper.InstantiateType(providerType, _log);
#pragma warning restore 8600
                    if (instance != null)
                    {
                        _log.LogInformation($"Found VSP provider implementation: ({providerType.FullName},{instance.ProviderType})");
                        _providerTypesMap[instance.ProviderType] = providerType;
                    }
                    else
                    {
                        _log.LogWarning($"Failed to instantiate a virtual storage provider of type: [{providerType.FullName}]");
                        _log.LogWarning($"This will likely be as a result of a problem in the virtual storage provider implementation");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IVirtualStorageManager.VirtualStorageManagerException(500, "Failed to load VSP providers", ex);
            }
        }
        
       
        /// <inheritdoc cref="IVirtualStorageManager.GetBindings"/>
        public List<VirtualStorageBinding> GetBindings()
        {
            return _virtualStorageConfiguration.Bindings;
        }

        /// <summary>
        /// Looks up a <see cref="VirtualStorageBinding"/> based on a tag
        /// </summary>
        /// <param name="tag">The tag to use</param>
        /// <returns></returns>
        protected VirtualStorageBinding? GetBindingFromTag(string tag)
        {
            foreach (var binding in _virtualStorageConfiguration.Bindings)
            {
                if (binding.Tag == tag)
                {
                    return binding;
                }
            }

            return null;
        }

        /// <summary>
        /// Instantiates and then binds a <see cref="IVirtualStorageProvider"/> for a given tag
        /// </summary>
        /// <param name="tag">The tag for the provider</param>
        /// <returns></returns>
        /// <exception cref="IVirtualStorageManager.VirtualStorageManagerException"></exception>
        public IVirtualStorageProvider GetProvider(string tag)
        {
            _log.LogDebug($"Attempting instantiation of VSP provider with tag [{tag}]");
            var binding = GetBindingFromTag(tag);
            if (binding == null)
            {
                throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                    $"No virtual storage provider exists for the specified tag: {tag}");
            }
            else
            {
                try
                {
                    var provider = CreateProviderInstance(binding.ProviderType);
                    provider.Bind(binding);
                    return provider;
                }
                catch (IVirtualStorageProvider.VirtualStorageProviderException ex)
                {
                    _log.LogWarning($"Failed to instantiate and then bind to a provider with tag [{tag}]: {ex.Message}");
                    throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                        "Failed to instantiate and bind specified provider [{tag}]", ex);
                }
            } 
        }
    }
}
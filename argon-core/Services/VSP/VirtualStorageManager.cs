#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Core;
using JCS.Argon.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Services.VSP
{
    /// <summary>
    ///     Default implementation of the VSP registry
    /// </summary>
    public class VirtualStorageManager : IVirtualStorageManager
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<VirtualStorageManager>();

        /// <summary>
        ///     An injected, pre-configured instance of <see cref="HttpClient" />
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        ///     Internal cache of the types associated with different <see cref="IVirtualStorageProvider" /> implementations
        /// </summary>
        private readonly Dictionary<string, Type> _providerTypesMap = new();

        /// <summary>
        ///     The current IoC <see cref="IServiceProvider" />
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     The overall application configuration, used to extract VSP bindings
        /// </summary>
        private readonly VirtualStorageOptions _virtualStorageOptions;

        public VirtualStorageManager(IServiceProvider serviceProvider,
            HttpClient httpClient,
            IOptionsMonitor<ApiOptions> apiOptions)
        {
            LogMethodCall(_log);
            if (apiOptions == null) LogWarning(_log, "Options appear to be null - expect problems");
            _virtualStorageOptions = apiOptions.CurrentValue.VirtualStorageOptions;
            if (_virtualStorageOptions == null) LogWarning(_log, "No virtual storage options found - expect problems");
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
            ResolveProviders();
        }


        /// <inheritdoc cref="IVirtualStorageManager.GetBindings" />
        public List<VirtualStorageBinding> GetBindings()
        {
            LogMethodCall(_log);
            return _virtualStorageOptions.Bindings;
        }

        /// <summary>
        ///     Instantiates and then binds a <see cref="IVirtualStorageProvider" /> for a given tag
        /// </summary>
        /// <param name="tag">The tag for the provider</param>
        /// <returns></returns>
        /// <exception cref="IVirtualStorageManager.VirtualStorageManagerException"></exception>
        public IVirtualStorageProvider GetProviderByTag(string tag)
        {
            LogMethodCall(_log);
            LogDebug(_log, $"Attempting instantiation of VSP provider with tag [{tag}]");
            var binding = GetBindingFromTag(tag);
            if (binding == null)
            {
                throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                    $"No virtual storage provider exists for the specified tag: {tag}");
            }

            try
            {
                var provider = CreateProviderInstance(binding.ProviderType);
                provider.Bind(binding, (IDbCache) _serviceProvider.GetService(typeof(IDbCache))!, _httpClient);
                return provider;
            }
            catch (IVirtualStorageProvider.VirtualStorageProviderException ex)
            {
                LogWarning(_log, $"Failed to instantiate and then bind to a provider with tag [{tag}]: {ex.Message}");
                throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to instantiate and bind specified provider [{tag}]", ex);
            }
        }

        /// <summary>
        ///     Check whether a given provider type exists
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        protected bool ProviderExists(string providerType)
        {
            LogMethodCall(_log);
            LogVerbose(_log, $"Checking for existence of provider with type \"{providerType}\"");
            return _providerTypesMap.ContainsKey(providerType);
        }

        /// <summary>
        ///     Checks whether or not we have a provider binding for a specified tag
        /// </summary>
        /// <param name="tag">The tag for the provider</param>
        /// <returns></returns>
        protected bool BindingExistsForTag(string tag)
        {
            LogMethodCall(_log);
            return _virtualStorageOptions.Bindings.Any(b => b.Tag == tag);
        }

        /// <summary>
        ///     Create an instance of a <see cref="IVirtualStorageProvider" /> based on the short
        ///     providerType value
        /// </summary>
        /// <param name="providerType"></param>
        /// a
        /// <returns></returns>
        /// <exception cref="IVirtualStorageManager.VirtualStorageManagerException"></exception>
        protected IVirtualStorageProvider CreateProviderInstance(string providerType)
        {
            LogMethodCall(_log);
            LogDebug(_log, $"Instantiating a virtual storage provider of type [{providerType}]");
            if (ProviderExists(providerType))
            {
                #pragma warning disable 8600
                var instance =
                    (IVirtualStorageProvider) ReflectionHelper.InstantiateType(_providerTypesMap[providerType]);
                #pragma warning restore 8600
                if (instance == null)
                {
                    throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                        $"Failed to instance a new instance of a virtual storage provider with type: [{providerType}]");
                }

                return instance;
            }

            throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                $"Request for a virtual storage provider which doesn't appear to exist: [{providerType}]");
        }


        /// <summary>
        ///     Scans the current runtime environment and looks for types that implement the <see cref="IVirtualStorageProvider" />
        ///     interface
        /// </summary>
        /// <exception cref="IVirtualStorageManager.VirtualStorageManagerException"></exception>
        protected void ResolveProviders()
        {
            LogMethodCall(_log);
            LogInformation(_log, "Resolving providers within the current runtime environment");
            try
            {
                var providerTypes = ReflectionHelper.LocateAllImplementors<IVirtualStorageProvider>()
                    .Where(t => !t.IsAbstract && !t.IsInterface);
                foreach (var providerType in providerTypes)
                {
                    #pragma warning disable 8600
                    var instance = (IVirtualStorageProvider) ReflectionHelper.InstantiateType(providerType);
                    #pragma warning restore 8600
                    if (instance != null)
                    {
                        LogInformation(_log, $"Found VSP provider implementation: ({providerType.Name},{instance.ProviderType})");
                        _providerTypesMap[instance.ProviderType] = providerType;
                    }
                    else
                    {
                        LogWarning(_log, $"Failed to instantiate a virtual storage provider of type: [{providerType.Name}]");
                        LogWarning(_log, "This will likely be as a result of a problem in the virtual storage provider implementation");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IVirtualStorageManager.VirtualStorageManagerException(500, "Failed to load VSP providers", ex);
            }
        }

        /// <summary>
        ///     Looks up a <see cref="VirtualStorageBinding" /> based on a tag
        /// </summary>
        /// <param name="tag">The tag to use</param>
        /// <returns></returns>
        protected VirtualStorageBinding? GetBindingFromTag(string tag)
        {
            LogMethodCall(_log);
            LogVerbose(_log, $"Scanning for binding with tag \"{tag}\"");
            foreach (var binding in _virtualStorageOptions.Bindings)
                if (binding.Tag == tag)
                    return binding;

            LogWarning(_log, $"Couldn't locate a binding with tag \"{tag}\"");
            return null;
        }
    }
}
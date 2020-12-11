using System;
using System.Net.Http;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Version = JCS.Argon.Model.Schema.Version;

namespace JCS.Argon.Services.VSP.Providers
{
    /// <summary>
    /// Base class that can be used to implement new VSP provider classes
    /// </summary>
    public abstract class BaseVirtualStorageProvider : IVirtualStorageProvider, IDisposable
    {
        /// <summary>
        /// An enumeration of "stock" provider properties - returned in the property bag for certain operations
        /// </summary>
        public enum ProviderProperties 
        {
            Path,
            CreateDate,
            LastAccessed,
            Length,
            ContentType
        }

        /// <summary>
        /// Copy of the current binding
        /// </summary>
        protected VirtualStorageBinding? _binding;

        /// <summary>
        /// The logger
        /// </summary>
        protected ILogger _log;

        /// <summary>
        /// A captured instance of <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider? _serviceProvider;

        /// <summary>
        /// An instance of <see cref="HttpClient"/>
        /// </summary>
        protected HttpClient _httpClient;

        /// <summary>
        /// Default constructor required for dynamic instantiation
        /// </summary>
        protected BaseVirtualStorageProvider(ILogger log)
        {
            _log = log;
            _binding = null;
        }

        /// <summary>
        /// Equiv of a virtual destructor
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <inheritdoc cref="IVirtualStorageProvider.Binding"/>
        public VirtualStorageBinding? Binding => _binding;

        /// <inheritdoc cref="IVirtualStorageProvider.ProviderType"/>
        public abstract string ProviderType { get; }

        /// <inheritdoc cref="IVirtualStorageProvider.Bind"/> 
        public void Bind(VirtualStorageBinding binding, IServiceProvider serviceProvider, HttpClient httpClient)
        {
            _binding = binding;
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
            _log.LogDebug($"Performing VSP bind: {_binding}");
            AfterBind();
        }

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionAsync"/>
        public abstract Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection);

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionItemVersionAsync"/>
        public abstract Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, Version version, IFormFile source);

        /// <inheritdoc cref="IVirtualStorageProvider.ReadCollectionItemVersionAsync"/>
        public abstract Task<IVirtualStorageProvider.StorageOperationResult> ReadCollectionItemVersionAsync(Collection collection, Item item, Version version);

        /// <summary>
        /// Called after a bind operation - subclasses should perform initialisation logic in
        /// here
        /// </summary>
        public abstract void AfterBind();
    }
}
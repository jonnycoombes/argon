using System;
using System.Net.Http;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Core;
using Microsoft.AspNetCore.Http;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Services.VSP.Providers
{
    /// <summary>
    /// Base class that can be used to implement new VSP provider classes
    /// </summary>
    public abstract class BaseVirtualStorageProvider : IVirtualStorageProvider, IDisposable
    {

        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext<BaseVirtualStorageProvider>();
        
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
        protected VirtualStorageBinding _binding = null!;

        /// <summary>
        /// An instance of <see cref="HttpClient"/>
        /// </summary>
        protected HttpClient _httpClient= null!;

        /// <summary>
        /// An instance of <see cref="IDbCache"/>
        /// </summary>
        protected IDbCache _dbCache = null!;

        /// <summary>
        /// Default constructor required for dynamic instantiation
        /// </summary>
        protected BaseVirtualStorageProvider()
        {
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
        public void Bind(VirtualStorageBinding binding, IDbCache dbCache, HttpClient httpClient)
        {
            LogMethodCall(_log);
            _binding = binding;
            _dbCache = dbCache;
            _httpClient = httpClient;
            AfterBind();
        }

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionAsync"/>
        public abstract Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection);

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionItemVersionAsync"/>
        public abstract Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, ItemVersion itemVersion, IFormFile source);

        /// <inheritdoc cref="IVirtualStorageProvider.ReadCollectionItemVersionAsync"/>
        public abstract Task<IVirtualStorageProvider.StorageOperationResult> ReadCollectionItemVersionAsync(Collection collection, Item item, ItemVersion itemVersion);

        /// <summary>
        /// Called after a bind operation - subclasses should perform initialisation logic in
        /// here
        /// </summary>
        public abstract void AfterBind();
    }
}
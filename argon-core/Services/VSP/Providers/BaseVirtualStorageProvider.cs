using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
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
        
        /// <inheritdoc cref="IVirtualStorageProvider.Binding"/>
        public VirtualStorageBinding? Binding => _binding;
        
        /// <inheritdoc cref="IVirtualStorageProvider.ProviderType"/>
        public abstract string ProviderType { get; }

        /// <inheritdoc cref="IVirtualStorageProvider.Bind"/> 
        public void Bind(VirtualStorageBinding binding)
        {
            _binding = binding;
            _log.LogDebug($"Performing VSP bind: {_binding}");
            AfterBind();
        }


        /// <summary>
        /// Default constructor required for dynamic instantiation
        /// </summary>
        protected BaseVirtualStorageProvider(ILogger log)
        {
            _log = log;
            _binding = null;
        }

        /// <summary>
        /// Called after a bind operation - subclasses should perform initialisation logic in
        /// here
        /// </summary>
        public abstract void AfterBind();
        
        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionAsync"/>
        public abstract Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection);
        
        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionItemAsync"/>
        public abstract Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemAsync(Collection collection,
            Item item, IFormFile source);
        
        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionItemVersionAsync"/>
        public abstract Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, Version version, FileStream source);
        
        
        /// <summary>
        /// Equiv of a virtual destructor
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
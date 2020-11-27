using System;
using System.Dynamic;
using JCS.Argon.Model.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace JCS.Argon.Services.VSP.Providers
{
    /// <summary>
    /// Base class that can be used to implement new VSP provider classes
    /// </summary>
    public abstract class BaseVirtualStorageProvider : IVirtualStorageProvider, IDisposable
    {
        /// <summary>
        /// Copy of the current binding
        /// </summary>
        protected VirtualStorageBinding _binding;

        protected ILogger _log;
        
        /// <inheritdoc cref="IVirtualStorageProvider.Binding"/>
        public VirtualStorageBinding Binding => _binding;
        
        /// <inheritdoc cref="IVirtualStorageProvider.ProviderType"/>
        public abstract string ProviderType { get; }

        /// <inheritdoc cref="IVirtualStorageProvider.Bind"/> 
        public void Bind(VirtualStorageBinding binding)
        {
            _log.LogDebug($"Performing VSP bind: {_binding}");
            _binding = binding;
            AfterBind();
        }

        /// <summary>
        /// Default constructor required for dynamic instantiation
        /// </summary>
        protected BaseVirtualStorageProvider(ILogger log)
        {
            _log = log;
        }

        /// <summary>
        /// Called after a bind operation - subclasses should perform initialisation logic in
        /// here
        /// </summary>
        public abstract void AfterBind();
        
        /// <summary>
        /// Equiv of a virtual destructor
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
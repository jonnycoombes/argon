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
    public abstract class BaseVSPProvider : IVSPProvider, IDisposable
    {
        /// <summary>
        /// Copy of the current binding
        /// </summary>
        protected VSPBinding _binding;

        protected ILogger _log;
        
        /// <inheritdoc cref="IVSPProvider.Binding"/>
        public VSPBinding Binding => _binding;
        
        /// <inheritdoc cref="IVSPProvider.ProviderType"/>
        public abstract string ProviderType { get; }

        /// <inheritdoc cref="IVSPProvider.Bind"/> 
        public void Bind(VSPBinding binding)
        {
            _log.LogDebug($"Performing VSP bind: {_binding}");
            _binding = binding;
            AfterBind();
        }

        /// <summary>
        /// Default constructor required for dynamic instantiation
        /// </summary>
        protected BaseVSPProvider(ILogger log)
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
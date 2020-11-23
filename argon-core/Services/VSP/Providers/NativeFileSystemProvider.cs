using JCS.Argon.Model.Configuration;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.VSP.Providers
{
    /// <summary>
    /// VSP provider that maps to an underlying file system structure
    /// </summary>
    public class NativeFileSystemProvider : BaseVSPProvider 
    {
        public override string ProviderType => "nativeFs";
        
        /// <summary>
        /// Default constructor, just calls base
        /// </summary>
        /// <param name="log"></param>
        public NativeFileSystemProvider(ILogger log) : base(log) 
        {
            
        }
        
        /// <inheritdoc cref="BaseVSPProvider.AfterBind"/> 
        public override void AfterBind()
        {
            _log.LogDebug($"{ProviderType}: AfterBind called - performing initialisation ");
        }
    }
}
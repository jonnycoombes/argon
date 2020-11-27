using JCS.Argon.Model.Configuration;

namespace JCS.Argon.Services.VSP
{
    /// <summary>
    /// The core interface that must be implemented by each distinct VSP provider
    /// </summary>
    public interface IVirtualStorageProvider
    {
        /// <summary>
        /// Read-only property that contains the current <see cref="VirtualStorageBinding"/>
        /// </summary>
        VirtualStorageBinding Binding { get; }
        
        /// <summary>
        /// Should return a unique identifying string for the provider.  This is used within
        /// <see cref="VirtualStorageBinding"/> configuration elements in order to tell instances of <see cref="IVirtualStorageManager"/>
        /// what class to load, and then to validate the configuration
        /// </summary>
        string ProviderType { get; }

        /// <summary>
        /// Binds a given provider to its configuration
        /// </summary>
        /// <param name="binding"></param>
        public void Bind(VirtualStorageBinding binding);

    }
}
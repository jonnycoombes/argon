using System;
using JCS.Argon.Model.Configuration;

namespace JCS.Argon.Services.VSP
{
    /// <summary>
    /// The core interface that must be implemented by each distinct VSP provider
    /// </summary>
    public interface IVirtualStorageProvider
    {

        public enum StorageOperationStatus
        {
            Ok,
            Failed
        }
        
        /// <summary>
        /// Class used to standardise responses back from the virtual storage provider layer
        /// </summary>
        public class StorageOperationResult
        {
            /// <summary>
            /// A required operation status code
            /// </summary>
            public StorageOperationStatus Status { get; set; } = StorageOperationStatus.Ok;
            
            /// <summary>
            /// A uri relating to the operation
            /// </summary>
            public Uri? Uri { get; set; }
            
            /// <summary>
            /// An optional size property
            /// </summary>
            public long? Size { get; set; }

            /// <summary>
            /// Default constructor
            /// </summary>
            public StorageOperationResult()
            {
                
            }
            
        }

        /// <summary>
        /// Read-only property that contains the current <see cref="VirtualStorageBinding"/>
        /// </summary>
        VirtualStorageBinding? Binding { get; }
        
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
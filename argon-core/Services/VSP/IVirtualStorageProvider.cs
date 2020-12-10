using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Version = JCS.Argon.Model.Schema.Version;

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
        /// <param name="serviceProvider"></param>
        public void Bind(VirtualStorageBinding binding, IServiceProvider serviceProvider);

        /// <summary>
        /// Asks a provider to create a physical representation of a <see cref="Collection"/>
        /// </summary>
        /// <param name="collection">The entity associated with the collection.  This may be mutated by this operation</param>
        /// <returns></returns>
        public Task<StorageOperationResult> CreateCollectionAsync(Collection collection);

        /// <summary>
        /// Given an <see cref="Item"/> and <see cref="Version"/> instance, create a physical representation of a
        /// specific version
        /// </summary>
        /// <param name="collection">The parent collection for the version</param>
        /// <param name="item">The item model element</param>
        /// <param name="version">The version model element</param>
        /// <param name="source">A filestream containing the source for the version</param>
        /// <returns></returns>
        public Task<StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, Version version, IFormFile source);

        /// <summary>
        /// Reads a specific item version from a collection 
        /// </summary>
        /// <param name="collection">The collection object</param>
        /// <param name="item">The item object</param>
        /// <param name="version">The version object</param>
        /// <returns></returns>
        public Task<StorageOperationResult> ReadCollectionItemVersionAsync(Collection collection, Item item, Version version);

        public sealed class VirtualStorageProviderException : ResponseAwareException
        {
            public VirtualStorageProviderException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(IVirtualStorageProvider);
            }

            public VirtualStorageProviderException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(IVirtualStorageProvider);
            }
        }

        /// <summary>
        /// Class used to standardise responses back from the virtual storage provider layer
        /// </summary>
        public class StorageOperationResult
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            public StorageOperationResult()
            {
                
            }

            /// <summary>
            /// A required operation status code
            /// </summary>
            public StorageOperationStatus Status { get; set; } = StorageOperationStatus.Ok;

            /// <summary>
            /// And optional set of properties that may or may not be merged into
            /// <see cref="PropertyGroup"/> instances
            /// </summary>
            public Dictionary<string, object>? Properties { get; set; }

            /// <summary>
            /// An optional stream object which may be returned by certain operations
            /// </summary>
            public Stream? Stream { get; set; }

            /// <summary>
            /// An optional error message that can be passed back up through the stack
            /// In general, only used if some kind of 'retryable' error occurs
            /// </summary>
            public string? ErrorMessage { get; set; }

            /// <summary>
            /// An optional size property
            /// </summary>
            public long? Size { get; set; }
        }
    }
}
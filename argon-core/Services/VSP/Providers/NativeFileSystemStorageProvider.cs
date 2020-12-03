using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.VSP.Providers
{
    /// <summary>
    /// VSP provider that maps to an underlying file system structure
    /// </summary>
    public class NativeFileSystemStorageProvider : BaseVirtualStorageProvider 
    {
        public override string ProviderType => "nativeFileSystem";

        /// <summary>
        /// The property which contains the root path for the collection
        /// </summary>
        protected static string ROOTPATH_PROPERTY = "rootPath";

        /// <summary>
        /// Used for stashing properties
        /// </summary>
        protected static string COLLECTION_PROPERTY_PREFIX = "nativeFS";

        /// <summary>
        /// Used to denote the path of the collection
        /// </summary>
        protected static string COLLECTION_PATH_PROPERTY = "path";

        /// <summary>
        /// The current configured root path info
        /// </summary>
        protected DirectoryInfo _rootPathInfo;
        
        /// <summary>
        /// Default constructor, just calls base
        /// </summary>
        /// <param name="log"></param>
        public NativeFileSystemStorageProvider(ILogger log) : base(log) 
        {
            
        }

        /// Validate and check for any mandatory binding configuraton properties.
        /// On failure, throw a <see cref="IVirtualStorageProvider.VirtualStorageProviderException"/>
        /// <inheritdoc cref="BaseVirtualStorageProvider.AfterBind"/> 
        public override void AfterBind()
        {
            _log.LogDebug($"{ProviderType}: AfterBind called - performing initialisation ");
            if (!_binding!.Properties.ContainsKey(ROOTPATH_PROPERTY))
            {
                throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                    $"{ROOTPATH_PROPERTY} not found in binding configuration for this provider");
            }
            else
            {
                _rootPathInfo = new DirectoryInfo(@$"{(string)_binding!.Properties[ROOTPATH_PROPERTY]}");
                if (!Directory.Exists(_rootPathInfo.FullName))
                {
                    throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                        $"Specified root path doesn't exist or is not accessible: {_rootPathInfo.FullName}");
                }
            }
        }

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionAsync"/>
        public override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            var result = new IVirtualStorageProvider.StorageOperationResult();
            var collectionPath = Path.Combine(_rootPathInfo.FullName, collection.Id.ToString()!);
            if (Directory.Exists(collectionPath))
            {
                throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                    $"Computed collection storage path already exists! {collectionPath}");
            }
            else
            {
                return Task.Run(() =>
                {
                    _log.LogDebug($"Creating a new collection storage root at {collectionPath}");
                    var info = Directory.CreateDirectory(collectionPath);
                    result.Status = IVirtualStorageProvider.StorageOperationStatus.Ok;
                    result.Properties = new Dictionary<string, object>()
                    {
                        {$"{COLLECTION_PROPERTY_PREFIX}.{COLLECTION_PATH_PROPERTY}", collectionPath}
                    };
                    return result;
                });
            }
        }

        public override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemAsync(Collection collection, Item item, FileStream source)
        {
            throw new System.NotImplementedException();
        }

        public override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, Version version, FileStream source)
        {
            throw new System.NotImplementedException();
        }
    }
}
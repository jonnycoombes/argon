using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Version = JCS.Argon.Model.Schema.Version;

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
        protected static string PATH_PROPERTY = "path";

        /// <summary>
        /// The creation date/time for the collection
        /// </summary>
        protected static string CREATE_DATETIME_PROPERTY = "createDateTime";
       
        /// <summary>
        /// Last accessed time
        /// </summary>
        protected static string LASTACCESS_DATETIME_PROPERTY = "lastAccessDateTime";

        /// <summary>
        /// The total filesize
        /// </summary>
        protected static string FILESIZE_PROPERTY = "sizeInBytes";

        protected static string CONTENTTYPE_PROPERTY = "contentType";

        /// <summary>
        /// The current configured root path info
        /// </summary>
        protected DirectoryInfo? _rootPathInfo;

        /// <summary>
        /// Utility for extracting the root path for a given collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private string CollectionRootPath(Collection collection)
        {
            return Path.Combine(_rootPathInfo!.FullName, collection.Id.ToString()!);
        }
        
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
                    try
                    {
                        _log.LogInformation($"Current root storage location of {_rootPathInfo.FullName} doesn't exist - trying to create it");
                        _rootPathInfo = Directory.CreateDirectory(_rootPathInfo.FullName);
                    }
                    catch (Exception ex)
                    {
                        throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                            $"Specified root path doesn't exist or is not accessible: {_rootPathInfo.FullName}");
                    }
                }
            }
        }

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionAsync"/>
        public override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            var result = new IVirtualStorageProvider.StorageOperationResult();
            var collectionRootPath = CollectionRootPath(collection); 
            if (Directory.Exists(collectionRootPath))
            {
                throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                    $"Computed collection storage path already exists! {collectionRootPath}");
            }
            else
            {
                return Task.Run(() =>
                {
                    _log.LogDebug($"Creating a new collection storage root at {collectionRootPath}");
                    var info = Directory.CreateDirectory(collectionRootPath);
                    result.Status = IVirtualStorageProvider.StorageOperationStatus.Ok;
                    result.Properties = new Dictionary<string, object>()
                    {
                        {$"{COLLECTION_PROPERTY_PREFIX}.{PATH_PROPERTY}", collectionRootPath},
                        {$"{COLLECTION_PROPERTY_PREFIX}.{CREATE_DATETIME_PROPERTY}", info.CreationTimeUtc},
                        {$"{COLLECTION_PROPERTY_PREFIX}.{LASTACCESS_DATETIME_PROPERTY}", info.LastAccessTimeUtc},
                    };
                    return result;
                });
            }
        }

        public async override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemAsync(Collection collection,
            Item item,
            IFormFile source)
        {
            var collectionRootPath = CollectionRootPath(collection);
            if (!Directory.Exists(collectionRootPath))
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    $"Specified collection root doesn't exist or isn't accessible");
            }
            else
            {
                try
                {
                    var extension = Path.GetExtension(source.FileName);
                    var itemPath = Path.Combine(collectionRootPath, $"{item.Id.ToString()!}{extension}");
                    var outStream = File.Create(itemPath);
                    await source.CopyToAsync(outStream);
                    var result = new IVirtualStorageProvider.StorageOperationResult();
                    result.Status = IVirtualStorageProvider.StorageOperationStatus.Ok;
                    result.Properties = new Dictionary<string, object>()
                    {
                        {$"{COLLECTION_PROPERTY_PREFIX}.{PATH_PROPERTY}", itemPath},
                        {$"{COLLECTION_PROPERTY_PREFIX}.{CREATE_DATETIME_PROPERTY}", DateTime.Now},
                        {$"{COLLECTION_PROPERTY_PREFIX}.{LASTACCESS_DATETIME_PROPERTY}", DateTime.Now},
                        {$"{COLLECTION_PROPERTY_PREFIX}.{FILESIZE_PROPERTY}", source.Length},
                        {$"{COLLECTION_PROPERTY_PREFIX}.{CONTENTTYPE_PROPERTY}", source.ContentType},
                    };
                    return result;
                }
                catch (Exception ex)
                {
                    _log.LogWarning($"Caught exception whilst attempting to write down a collection item: {ex.Message}");
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                        $"Caught an exception whilst attempting to write down a collection item", ex);
                }
            }
        }

        public override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, Version version, FileStream source)
        {
            throw new System.NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Version = JCS.Argon.Model.Schema.Version;

namespace JCS.Argon.Services.VSP.Providers
{
    /// <summary>
    /// VSP provider that maps to an underlying file system structure
    /// </summary>
    public class NativeFileSystemStorageProvider : BaseVirtualStorageProvider 
    {
        /// <summary>
        /// The property which contains the root path for the collection
        /// </summary>
        protected static string ROOTPATH_PROPERTY = "rootPath";

        /// <summary>
        /// 
        /// </summary>
        protected DirectoryInfo? _rootPathInfo;

        /// <summary>
        /// Default constructor, just calls base
        /// </summary>
        /// <param name="log"></param>
        public NativeFileSystemStorageProvider(ILogger log) : base(log) 
        {
            
        }

        public override string ProviderType => "nativeFileSystem";

        /// <summary>
        /// Utility for extracting the root path for a given collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        protected string GenerateCollectionPath(Collection collection)
        {
            return Path.Combine(_rootPathInfo!.FullName, collection.Id.ToString()!);
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
                _log.LogInformation($"{ProviderType}: Current root storage location set to be {_rootPathInfo}");
                if (!Directory.Exists(_rootPathInfo.FullName))
                {
                    try
                    {
                        _log.LogInformation($"{ProviderType}: Current root storage location of {_rootPathInfo.FullName} doesn't exist - trying to create it");
                        _rootPathInfo = Directory.CreateDirectory(_rootPathInfo.FullName);
                    }
                    catch (Exception ex)
                    {
                        throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                            $"Unable to create new root location for collections at {_rootPathInfo.FullName}: {ex.Message}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to determine the path of a given <see cref="Item"/> object
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected string? GetItemPathFromProperties(Item item)
        {
            if (item.PropertyGroup.HasProperty(ProviderProperties.Path.ToString()))
            {
                var prop = item.PropertyGroup.GetPropertyByName(ProviderProperties.Path.ToString());
                return prop?.StringValue;
            }
            else
            {
                _log.LogWarning($"{ProviderType}: The expected path property wasn't found against item with id {item.Id}");
                return null;
            }
        }

        /// <summary>
        /// Tries to determine the path of a given <see cref="Model.Schema.Version"/> object
        /// </summary>
        /// <param name="item"></param>
        /// <param name="version"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        protected string GenerateVersionPath(Collection collection, Item item, Version version)
        {
            var itemPath = GenerateItemStoragePath(collection, item); 
            return Path.Combine(itemPath, $"{version.Major}_{version.Minor}");
        }

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionAsync"/>
        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            var result = new IVirtualStorageProvider.StorageOperationResult();
            var collectionRootPath = GenerateCollectionPath(collection); 
            if (Directory.Exists(collectionRootPath))
            {
                throw new IVirtualStorageManager.VirtualStorageManagerException(StatusCodes.Status500InternalServerError,
                    $"Computed collection storage path already exists! {collectionRootPath}");
            }
            else
            {
                return await Task.Run(() =>
                {
                    _log.LogDebug($"{ProviderType}: Creating a new collection storage root at {collectionRootPath}");
                    var info = Directory.CreateDirectory(collectionRootPath);
                    result.Status = IVirtualStorageProvider.StorageOperationStatus.Ok;
                    result.Properties = new Dictionary<string, object>()
                    {
                        {$"{ProviderProperties.Path}", collectionRootPath},
                        {$"{ProviderProperties.CreateDate}", info.CreationTimeUtc},
                        {$"{ProviderProperties.LastAccessed}", info.LastAccessTimeUtc},
                    };
                    return result;
                });
            }
        }

        /// <summary>
        /// Generates the storage path for a given item
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected string GenerateItemStoragePath(Collection collection, Item item)
        {
            var collectionRootPath = GenerateCollectionPath(collection);
            return Path.Combine(collectionRootPath, item.Id.ToString()!);
        }

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionItemVersionAsync"/>
        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, Version version, IFormFile source)
        {
            try
            {
                var itemStoragePath = GenerateItemStoragePath(collection, item);
                var versionPath = GenerateVersionPath(collection, item, version);
                Directory.CreateDirectory(itemStoragePath);
                using var target = File.Create(versionPath);
                await source.CopyToAsync(target);
                var result = new IVirtualStorageProvider.StorageOperationResult();
                result.Status = IVirtualStorageProvider.StorageOperationStatus.Ok;
                result.Properties = new Dictionary<string, object>()
                {
                    {$"{ProviderProperties.Path}", itemStoragePath},
                    {$"{ProviderProperties.CreateDate}", DateTime.Now},
                    {$"{ProviderProperties.LastAccessed}", DateTime.Now},
                    {$"{ProviderProperties.Length}", source.Length},
                    {$"{ProviderProperties.ContentType}", source.ContentType},
                };
                return result;
            }
            catch (Exception ex)
            {
                _log.LogWarning($"Caught exception whilst attempting to add a new version");
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    $"Unable to add new version: {ex.Message}", ex);
            }

            throw new NotImplementedException();
        }

        public override async Task<IVirtualStorageProvider.StorageOperationResult> ReadCollectionItemVersionAsync(Collection collection, Item item, Version version)
        {
            var itemStoragePath = GenerateItemStoragePath(collection, item);
            var versionStoragePath = GenerateVersionPath(collection, item, version);
            if (!File.Exists(versionStoragePath))
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    $"The specified version storage location doesn't exist, when it should: {versionStoragePath}");
            }

            var stream = new FileStream(versionStoragePath, FileMode.Open);
            return await Task.Run(() =>
            {
                return new IVirtualStorageProvider.StorageOperationResult()
                {
                    Status = IVirtualStorageProvider.StorageOperationStatus.Ok,
                    Stream = stream
                };
            });
        }
    }
}
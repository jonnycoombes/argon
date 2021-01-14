using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Schema;
using JCS.Argon.Utility;
using Microsoft.AspNetCore.Http;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Services.VSP.Providers
{
    public class OpenTextRestStorageProvider : BaseVirtualStorageProvider
    {
        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext<OpenTextRestStorageProvider>();
        
        /// <summary>
        /// The constant provider type tag
        /// </summary>
        public override string ProviderType => "openTextRestful";

        /// <summary>
        /// An internal instance of <see cref="OpenTextRestClient"/>
        /// </summary>
        private OpenTextRestClient _client= null!;

        /// <summary>
        /// The root storage path, relative to the Enterprise workspace
        /// </summary>
        private string? _rootCollectionPath; 
        
        /// <summary>
        /// Default constructor - since providers are loaded dynamically at run time,
        /// this is really just delegated to the base class
        /// </summary>
        /// <param name="log"></param>
        public OpenTextRestStorageProvider() : base()
        {
            LogMethodCall(_log);
        }

        /// <summary>
        /// Called after an instance of this provider has been bound, so in here we create a new instance
        /// of an <see cref="OpenTextRestClient"/>
        /// </summary>
        public override void AfterBind()
        {
            LogMethodCall(_log);
            _rootCollectionPath = Binding!.Properties["rootCollectionPath"].ToString()!;
            this.AssertNotNull(_rootCollectionPath, "Root path hasn't been specified!");
            LogDebug(_log,$"{ProviderType}: rootCollectionPath set to {_rootCollectionPath}");
            _client = CreateRestClient();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private string GenerateCollectionPath(Collection collection)
        {
            LogMethodCall(_log);
            return $"{_rootCollectionPath!}/{collection.Id.ToString()!}";
        }

        private string GenerateItemPath(Collection collection, Item item)
        {
            LogMethodCall(_log);
            return $"{_rootCollectionPath}/{collection.Id.ToString()}/{item.Id.ToString()}";
        }

        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            LogMethodCall(_log);
            try
            {
                await _client.Authenticate();
                var collectionRootPath = GenerateCollectionPath(collection);
                var nodeId= await _client.CreatePath(GenerateCollectionPath(collection));
                if (nodeId == null)
                {
                    return new IVirtualStorageProvider.StorageOperationResult()
                    {
                        Status = IVirtualStorageProvider.StorageOperationStatus.Failed
                    };
                }
                else
                {
                    var result = new IVirtualStorageProvider.StorageOperationResult();
                    result.Status = IVirtualStorageProvider.StorageOperationStatus.Ok;
                    result.Properties = new Dictionary<string, object>()
                    {
                        {$"{ProviderProperties.Path}", collectionRootPath},
                        {$"{ProviderProperties.CreateDate}", DateTime.Now},
                        {$"{ProviderProperties.LastAccessed}", DateTime.Now},
                        {"nodeId", nodeId}
                    };
                    return result;
                }
            }
            catch (OpenTextRestClient.OpenTextRestClientException ex)
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(ex.ResponseCodeHint,
                    $"An exception was caught from within the OTCS REST layer: {ex.GetBaseException().Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new OpenText rest client
        /// </summary>
        /// <returns></returns>
        private OpenTextRestClient CreateRestClient()
        {
            LogMethodCall(_log);
            this.AssertNotNull(_httpClient, "HTTP client has not been injected!");
            var client = new OpenTextRestClient(_dbCache)
            {
                CachePartition = Binding!.Tag,
                EndpointAddress = Binding!.Properties["endpoint"].ToString(),
                UserName = Binding!.Properties["user"].ToString(),
                Password = Binding!.Properties["password"].ToString(),
                HttpClient = _httpClient
            };
            return client;
        }

        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, ItemVersion itemVersion, IFormFile source)
        {
            LogMethodCall(_log);
            if (collection != null && !collection.PropertyGroup.HasProperty("nodeId"))
            {
                throw new OpenTextRestClient.OpenTextRestClientException(StatusCodes.Status400BadRequest,
                    $"Unable to locate cached node id for collection");
            }
            else
            {
                long itemNodeId;
                await _client.Authenticate();
                var collectionNodeId = (long) collection.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
                if (item.PropertyGroup.HasProperty("nodeId"))
                {
                    itemNodeId = (long)item.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
                }
                else if (await _client.HasChildFolder(collectionNodeId, item.Id.ToString()) == false)
                {
                    itemNodeId= await _client.CreateFolder(collectionNodeId, item.Id.ToString(), item.Name);
                }
                else
                {
                    itemNodeId= await _client.GetChildFolderId(collectionNodeId, item.Id.ToString());
                }
                if (itemNodeId != 0)
                {
                    item.PropertyGroup.AddOrReplaceProperty("nodeId", PropertyType.Number, itemNodeId);
                    var versionNodeId = await _client.CreateFolder(itemNodeId, $"{itemVersion.Major}_{itemVersion.Minor}", itemVersion.Name);
                    if (versionNodeId != 0)
                    {
                        var fileId = await _client.UploadFile(versionNodeId, source.FileName, source.FileName, source.OpenReadStream());
                        return new IVirtualStorageProvider.StorageOperationResult()
                        {
                            Status = IVirtualStorageProvider.StorageOperationStatus.Ok
                        };
                    }
                    else
                    {
                        throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status400BadRequest, 
                            $"Unable to create/locate node id");
                    }
                }
                else
                {
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status400BadRequest, 
                        $"Unable to create/locate node id");
                }
            }
        }

        public override async Task<IVirtualStorageProvider.StorageOperationResult> ReadCollectionItemVersionAsync(Collection collection, Item item, ItemVersion itemVersion)
        {
            LogMethodCall(_log);
            throw new NotImplementedException();
        }
    }
}
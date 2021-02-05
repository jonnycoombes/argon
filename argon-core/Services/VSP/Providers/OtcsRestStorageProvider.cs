#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Model.Schema;
using JCS.Argon.Utility;
using Microsoft.AspNetCore.Http;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Services.VSP.Providers
{
    public class OtcsRestStorageProvider : BaseVirtualStorageProvider
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<OtcsRestStorageProvider>();

        /// <summary>
        ///     An internal instance of <see cref="OtcsRestClient" />
        /// </summary>
        private OtcsRestClient _client = null!;

        /// <summary>
        ///     The root storage path, relative to the Enterprise workspace
        /// </summary>
        private string? _rootCollectionPath;

        /// <summary>
        ///     Default constructor - since providers are loaded dynamically at run time,
        ///     this is really just delegated to the base class
        /// </summary>
        public OtcsRestStorageProvider()
        {
            LogMethodCall(_log);
        }

        /// <summary>
        ///     The constant provider type tag
        /// </summary>
        public override string ProviderType => "openTextRestful";

        /// <summary>
        ///     Called after an instance of this provider has been bound, so in here we create a new instance
        ///     of an <see cref="OtcsRestClient" />
        /// </summary>
        public override void AfterBind()
        {
            LogMethodCall(_log);
            _rootCollectionPath = Binding!.Properties["rootCollectionPath"].ToString()!;
            this.AssertNotNull(_rootCollectionPath, "Root path hasn't been specified!");
            LogDebug(_log, $"{ProviderType}: rootCollectionPath set to {_rootCollectionPath}");
            _client = CreateOtcsRestClient();
        }

        /// <summary>
        ///     Generates the path within the CS repository for a new collection
        /// </summary>
        /// <param name="collection">The <see cref="Collection" /></param>
        /// <returns></returns>
        private string GenerateCollectionPath(Collection collection)
        {
            LogMethodCall(_log);
            return $"{_rootCollectionPath!}/{collection.Id.ToString()!}";
        }

        /// <summary>
        ///     Generates the path within the CS repository for a new collection item
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /></param>
        /// <param name="item">The <see cref="Item" /></param>
        /// <returns></returns>
        private string GenerateItemPath(Collection collection, Item item)
        {
            LogMethodCall(_log);
            return $"{_rootCollectionPath}/{collection.Id.ToString()}/{item.Id.ToString()}";
        }

        /// <summary>
        ///     Generates the path for a given version of an item
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /></param>
        /// <param name="item">The parent <see cref="Item" /></param>
        /// <param name="itemVersion">The <see cref="ItemVersion" /> in question</param>
        /// <returns></returns>
        private string GenerateItemVersionPath(Collection collection, Item item, ItemVersion itemVersion)
        {
            return
                $"{_rootCollectionPath}/{collection.Id.ToString()}/{item.Id.ToString()}/{itemVersion.Major}_{itemVersion.Minor}/{item.Name}";
        }

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionAsync" />
        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            LogMethodCall(_log);
            try
            {
                await _client.Authenticate();
                var collectionRootPath = GenerateCollectionPath(collection);
                var nodeId = await _client.CreatePath(GenerateCollectionPath(collection));
                if (nodeId == null)
                    return new IVirtualStorageProvider.StorageOperationResult
                    {
                        Status = IVirtualStorageProvider.StorageOperationStatus.Failed
                    };

                var result = new IVirtualStorageProvider.StorageOperationResult
                {
                    Status = IVirtualStorageProvider.StorageOperationStatus.Ok,
                    Properties = new Dictionary<string, object>
                    {
                        {$"{ProviderProperties.Path}", collectionRootPath},
                        {$"{ProviderProperties.CreateDate}", DateTime.Now},
                        {$"{ProviderProperties.LastAccessed}", DateTime.Now},
                        {"nodeId", nodeId}
                    }
                };
                return result;
            }
            catch (OtcsRestClient.OpenTextRestClientException ex)
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(ex.ResponseCodeHint,
                    $"An exception was caught from within the OTCS REST layer: {ex.GetBaseException().Message}", ex);
            }
        }

        /// <summary>
        ///     Creates a new OpenText rest client
        /// </summary>
        /// <returns></returns>
        private OtcsRestClient CreateOtcsRestClient()
        {
            LogMethodCall(_log);
            this.AssertNotNull(_httpClient, "HTTP client has not been injected!");
            var authType = (string) _binding.Properties["authType"];
            OtcsRestClient client;
            if (authType == null || authType == "basic")
                client = new OtcsRestClient(_dbCache)
                {
                    OtcsAuthenticationType = OtcsRestClient.AuthenticationType.Basic,
                    CachePartition = Binding!.Tag,
                    EndpointAddress = Binding!.Properties["endpoint"].ToString(),
                    UserName = Binding!.Properties["user"].ToString(),
                    Password = Binding!.Properties["password"].ToString(),
                    HttpClient = _httpClient
                };
            else
                client = new OtcsRestClient(_dbCache)
                {
                    OtcsAuthenticationType = OtcsRestClient.AuthenticationType.Ntlm,
                    CachePartition = Binding!.Tag,
                    EndpointAddress = Binding!.Properties["endpoint"].ToString(),
                    HttpClient = _httpClient
                };

            return client;
        }

        /// <inheritdoc cref="IVirtualStorageProvider.CreateCollectionItemVersionAsync" />
        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection,
            Item item, ItemVersion itemVersion, IFormFile source)
        {
            LogMethodCall(_log);
            if (collection != null && !collection.PropertyGroup.HasProperty("nodeId"))
                throw new OtcsRestClient.OpenTextRestClientException(StatusCodes.Status400BadRequest,
                    "Unable to locate cached node id for collection");

            await _client.Authenticate();

            long itemNodeId;
            var collectionNodeId = (long) collection.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
            if (item.PropertyGroup.HasProperty("nodeId"))
                itemNodeId = (long) item.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
            else
                if (await _client.HasChildFolder(collectionNodeId, item.Id.ToString()) == false)
                    itemNodeId = await _client.CreateFolder(collectionNodeId, item.Id.ToString(), item.Name);
                else
                    itemNodeId = await _client.GetChildId(collectionNodeId, item.Id.ToString());

            if (itemNodeId == 0)
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status400BadRequest,
                    "Unable to create/locate node id");
            item.PropertyGroup.AddOrReplaceProperty("nodeId", PropertyType.Number, itemNodeId);
            var versionNodeId =
                await _client.CreateFolder(itemNodeId, $"{itemVersion.Major}_{itemVersion.Minor}", itemVersion.Name);
            if (versionNodeId == 0)
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status400BadRequest,
                    "Unable to create/locate node id");
            var fileId = await _client.UploadFile(versionNodeId, source.FileName, source.FileName, source.OpenReadStream());
            var result = new IVirtualStorageProvider.StorageOperationResult
            {
                Status = IVirtualStorageProvider.StorageOperationStatus.Ok
            };
            return result;
        }

        /// <inheritdoc cref="IVirtualStorageProvider.ReadCollectionItemVersionAsync" />
        public override async Task<IVirtualStorageProvider.StorageOperationResult> ReadCollectionItemVersionAsync(Collection collection,
            Item item, ItemVersion itemVersion)
        {
            LogMethodCall(_log);
            try
            {
                await _client.Authenticate();
                var itemNodeId = (long) item.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
                var versionFolderName = $"{itemVersion.Major}_{itemVersion.Minor}";
                var versionFolderId = await _client.GetChildId(itemNodeId, versionFolderName);
                var versionId = await _client.GetChildId(versionFolderId, itemVersion.Name);
                var stream = await _client.GetNodeVersionContent(versionId);
                var result = new IVirtualStorageProvider.StorageOperationResult
                {
                    Status = IVirtualStorageProvider.StorageOperationStatus.Ok, Stream = stream
                };
                return result;
            }
            catch
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    "Unable to retrieve item version");
            }
        }

        /// <inheritdoc cref="IVirtualStorageProvider.DeleteCollectionItemAsync" />
        public override async Task<IVirtualStorageProvider.StorageOperationResult> DeleteCollectionItemAsync(Collection collection, Item
            item)
        {
            LogMethodCall(_log);
            try
            {
                await _client.Authenticate();
                var itemNodeId = (long) item.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
                await _client.DeleteNode(itemNodeId);
                var result = new IVirtualStorageProvider.StorageOperationResult
                {
                    Status = IVirtualStorageProvider.StorageOperationStatus.Ok
                };
                return result;
            }
            catch
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    "Unable to retrieve item version");
            }
        }
    }
}
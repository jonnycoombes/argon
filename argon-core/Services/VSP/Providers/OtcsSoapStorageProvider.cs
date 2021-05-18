using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Soap.Opentext;
using Microsoft.AspNetCore.Http;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;
using static JCS.Neon.Glow.Helpers.General.ParseHelpers;

namespace JCS.Argon.Services.VSP.Providers
{
    /// <summary>
    ///     Virtual storage provider that translates collection and item operations to an underlying OTCS instance, utilising the
    ///     CWS SOAP-based API
    /// </summary>
    public class OtcsSoapStorageProvider : BaseVirtualStorageProvider
    {
        /// <summary>
        ///     Key in the binding properties which must contain the base endpoint address
        /// </summary>
        private const string EndpointPropertyKey = "endpoint";

        /// <summary>
        ///     Key in the binding properties which must contain a path for the collection root, relative to the Enterprise Workspace
        /// </summary>
        private const string RootCollectionPathPropertyKey = "rootCollectionPath";

        /// <summary>
        ///     Key in the binding properties which must contain a valid authentication type
        /// </summary>
        private const string AuthTypePropertyKey = "authType";

        /// <summary>
        ///     Key in the binding properties which may contain a user to use if basic authentication is selected
        /// </summary>
        private const string UserPropertyKey = "user";

        /// <summary>
        ///     Key in the binding properties which may contain a password to use if basic authentication is selected
        /// </summary>
        private const string PasswordPropertyKey = "password";

        /// <summary>
        ///     The never changing enterprise workspace node id
        /// </summary>
        private const long EnterpriseRootId = 2000;

        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<OtcsSoapStorageProvider>();

        /// <summary>
        ///     The <see cref="WebServiceClient" /> instance used to make outcalls through the various CWS services
        /// </summary>
        private WebServiceClient _client;

        /// <summary>
        ///     The root path.  All collections are stored underneath this location relative to the Enterprise workspace within OTCS
        /// </summary>
        private string rootCollectionPath;

        /// <summary>
        ///     The provider type tag for this type of provider - should correlate to settings within the appsettings.json file
        /// </summary>
        public override string ProviderType => "openTextSoap";

        /// <summary>
        ///     Attempts to retrieve or create a folder node based on a path relative to the Enterprise workspace
        /// </summary>
        /// <returns>The <see cref="Node" /> instance representing the root collections folder</returns>
        public async Task<Node?> GetOrCreateFolder(string[] pathElements)
        {
            Node folder = null;
            try
            {
                var currentFolderId = EnterpriseRootId;
                var existing = await _client.GetNodeByPath(EnterpriseRootId, pathElements);
                if (existing == null)
                {
                    foreach (var element in pathElements)
                    {
                        folder = await _client.GetNodeByName(currentFolderId, element) ??
                                 await _client.CreateFolder(currentFolderId, element);
                        currentFolderId = folder.ID;
                    }
                }
                else
                {
                    folder = existing;
                }
            }
            catch (WebServiceClientException ex)
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    $"OTCS Fault exception: {ex.Message}");
            }

            return folder;
        }

        /// <summary>
        /// </summary>
        /// <param name="collection">The <see cref="Collection" /> instance to create the underlying collection location/folder for</param>
        /// <returns>A valid <see cref="IVirtualStorageProvider.StorageOperationResult" /></returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException"></exception>
        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            LogMethodCall(_log);
            LogVerbose(_log, $"Creating new collection folder with id \"{collection.Id}\"");

            try
            {
                var collectionPath = $"{rootCollectionPath}/{collection.Id.ToString()}";
                var collectionFolderNode = await GetOrCreateFolder(collectionPath.Split("/"));
                if (collectionFolderNode != null)
                {
                    collectionFolderNode.Comment = collection.Name;
                    await _client.UpdateNode(collectionFolderNode);

                    var result = new IVirtualStorageProvider.StorageOperationResult
                    {
                        Status = IVirtualStorageProvider.StorageOperationStatus.Ok,
                        Properties = new Dictionary<string, object>
                        {
                            {$"{Collection.StockCollectionProperties.Path}", collectionPath},
                            {$"{Collection.StockCollectionProperties.CreateDate}", DateTime.Now},
                            {$"{Collection.StockCollectionProperties.LastAccessed}", DateTime.Now},
                            {"nodeId", collectionFolderNode.ID}
                        }
                    };
                    return result;
                }

                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    "Failed to create a new collection folder");
            }
            catch (WebServiceClientException ex)
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    $"OTCS Fault exception: {ex.Message}");
            }
        }

        /// <inheritdoc cref="IVirtualStorageProvider.DeleteCollectionAsync" />
        public override async Task<IVirtualStorageProvider.StorageOperationResult> DeleteCollectionAsync(Collection? collection)
        {
            LogMethodCall(_log);
            try
            {
                if (collection != null && !collection.PropertyGroup.HasProperty("nodeId"))
                {
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status400BadRequest,
                        "Unable to locate cached node id for collection");
                }

                var collectionNodeId = (long) collection.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
                await _client.DeleteNode(collectionNodeId);
            }
            catch (WebServiceClientException ex)
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    $"OTCS Fault exception: {ex.Message}");
            }

            return await Task.Run(() => new IVirtualStorageProvider.StorageOperationResult
            {
                Status = IVirtualStorageProvider.StorageOperationStatus.Ok
            });
        }

        /// <summary>
        ///     Uploads a new item to Content Server.  Note that this method uses the standard "attachment" method due to lack of support
        ///     for MTOM within the .NET Core version of WCF.
        /// </summary>
        /// <param name="parentOrExistingId">The parent id which will either be a folder (for a new document) or an actual document id (for a new version)</param>
        /// <param name="source">The <see cref="IFormFile" /> containing the source document material</param>
        /// <param name="addVersion">If set to true, the passed in id is interpreted as an existing document node</param>
        /// <returns></returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException"></exception>
        private async Task<long> UploadContent(long parentOrExistingId, IFormFile source, bool addVersion = false)
        {
            try
            {
                var buffer = new MemoryStream();
                await source.CopyToAsync(buffer);
                await buffer.FlushAsync();
                var attachment = new Attachment
                {
                    CreatedDate = DateTime.Now,
                    FileName = source.FileName,
                    FileSize = source.Length,
                    ModifiedDate = DateTime.Now,
                    Contents = buffer.ToArray()
                };

                if (!addVersion)
                {
                    var document = await _client.CreateDocument(parentOrExistingId, source.FileName, attachment);
                    return document.ID;
                }

                var version = await _client.AddVersion(parentOrExistingId, attachment);
                return parentOrExistingId;
            }
            catch (WebServiceClientException ex)
            {
                LogExceptionWarning(_log, ex);
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    $"OTCS item version creation failed \"{ex.Message}\"");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="itemVersion"></param>
        /// <param name="source"></param>
        /// <returns>A valid <see cref="IVirtualStorageProvider.StorageOperationResult" /></returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException"></exception>
        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection? collection,
            Item item, ItemVersion itemVersion, IFormFile source)
        {
            LogMethodCall(_log);

            // check we have the node id cached for the parent folder
            if (collection != null && !collection.PropertyGroup.HasProperty("nodeId"))
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status400BadRequest,
                    "Unable to locate cached node id for collection");
            }

            long cachedNodeId;
            long itemId;
            // check whether we already have an underlying CS document associated with this item
            if (item.PropertyGroup.HasProperty("nodeId"))
            {
                try
                {
                    // add a version
                    LogVerbose(_log, "Adding version to an existing OTCS item");
                    cachedNodeId = (long) item.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
                    itemId = await UploadContent(cachedNodeId, source, true);
                }
                catch (FaultException ex)
                {
                    LogExceptionWarning(_log, ex);
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                        $"OTCS item version creation failed \"{ex.Message}\"");
                }
            }
            else
            {
                try
                {
                    LogVerbose(_log, "Creating a new OTCS item");
                    var itemFolderPath = $"{rootCollectionPath}/{collection.Id.ToString()}/{item.Id.ToString()}";
                    var itemFolderNode = await GetOrCreateFolder(itemFolderPath.Split("/"));
                    if (itemFolderNode != null)
                    {
                        itemFolderNode.Comment = item.Name;
                        await _client.UpdateNode(itemFolderNode);
                        itemId = await UploadContent(itemFolderNode.ID, source);
                        item.PropertyGroup.AddOrReplaceProperty("nodeId", PropertyType.Number, itemId);
                    }
                }
                catch (FaultException ex)
                {
                    LogExceptionWarning(_log, ex);
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                        $"OTCS item creation failed \"{ex.Message}\"");
                }
            }

            var result = new IVirtualStorageProvider.StorageOperationResult
            {
                Status = IVirtualStorageProvider.StorageOperationStatus.Ok,
                Properties = new Dictionary<string, object>
                {
                    {$"{Collection.StockCollectionProperties.CreateDate}", DateTime.Now},
                    {$"{Collection.StockCollectionProperties.LastAccessed}", DateTime.Now},
                    {$"{Collection.StockCollectionProperties.Length}", source.Length},
                    {$"{Collection.StockCollectionProperties.ContentType}", DetermineContentType(source)}
                }
            };
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <returns>A valid <see cref="IVirtualStorageProvider.StorageOperationResult" /></returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException"></exception>
        public override async Task<IVirtualStorageProvider.StorageOperationResult> DeleteCollectionItemAsync(Collection collection,
            Item item)
        {
            Debug.Assert(_client != null);
            LogMethodCall(_log);
            if (item.PropertyGroup.HasProperty("nodeId"))
            {
                try
                {
                    var itemNodeId = (long) item.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
                    var itemNode = await _client.GetNode(itemNodeId);
                    await _client.DeleteNode(itemNodeId);
                    await _client.DeleteNode(itemNode.ParentID);
                    var result = new IVirtualStorageProvider.StorageOperationResult
                    {
                        Status = IVirtualStorageProvider.StorageOperationStatus.Ok
                    };
                    return result;
                }
                catch (FaultException ex)
                {
                    LogExceptionWarning(_log, ex);
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                        $"OTCS item deletion failed \"{ex.Message}\"");
                }
            }

            throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                "Specified item version doesn't appear to have a valid OTCS node id");
        }


        /// <summary>
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="itemVersion"></param>
        /// <returns>A valid <see cref="IVirtualStorageProvider.StorageOperationResult" /></returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException"></exception>
        public override async Task<IVirtualStorageProvider.StorageOperationResult> ReadCollectionItemVersionAsync(Collection collection,
            Item item, ItemVersion itemVersion)
        {
            LogMethodCall(_log);

            if (item.PropertyGroup.HasProperty("nodeId"))
            {
                try
                {
                    var itemNodeId = (long) item.PropertyGroup.GetPropertyByName("nodeId").NumberValue;
                    var attachment = await _client.GetVersionContents(itemNodeId, itemVersion.Major);
                    var result = new IVirtualStorageProvider.StorageOperationResult
                    {
                        Status = IVirtualStorageProvider.StorageOperationStatus.Ok,
                        Stream = new MemoryStream(attachment.Contents)
                    };
                    return result;
                }
                catch (WebServiceClientException ex)
                {
                    LogExceptionWarning(_log, ex);
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                        $"OTCS item deletion failed \"{ex.Message}\"");
                }
            }

            throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                "Specified item version doesn't appear to have a valid OTCS node id");
        }

        /// <summary>
        ///     Checks the current <see cref="JCS.Argon.Model.Configuration.VirtualStorageBinding" /> and then synthesises any additional configuration
        ///     information that may be required, such as the complete service endpoints.
        /// </summary>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException">Thrown in the case of an invalid configuration</exception>
        public override void AfterBind()
        {
            LogMethodCall(_log);

            string baseEndpointAddress;

            if (_binding.Properties.ContainsKey(EndpointPropertyKey))
            {
                baseEndpointAddress = (string) _binding.Properties[EndpointPropertyKey];
                if (!string.IsNullOrEmpty(baseEndpointAddress) && ParseUri(baseEndpointAddress).IsNone)
                {
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                        $"The specified base endpoint address doesn't look as if it's valid \"{baseEndpointAddress}\"");
                }

                // make sure that we have a trailing slash on the base endpoint address
                if (!baseEndpointAddress.EndsWith("/"))
                {
                    baseEndpointAddress = $"{baseEndpointAddress}/";
                }

                LogVerbose(_log, $"Base endpoint address being set to \"{baseEndpointAddress}\"");
            }
            else
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    "No endpoint has been specified, or can be found in the current configuration");
            }

            // check that we have a collection root and that it can be decomposed into one or more path components
            if (_binding.Properties.ContainsKey(RootCollectionPathPropertyKey))
            {
                rootCollectionPath = (string) _binding.Properties[RootCollectionPathPropertyKey];
                if (string.IsNullOrEmpty(rootCollectionPath))
                {
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                        "No collection root has been specified.  An OTCS collection root location must be specified, relative to Enterprise");
                }

                LogVerbose(_log, $"Collection root path set to \"{rootCollectionPath}\"");
            }
            else
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                    "No collection root has been specified.  An OTCS collection root location must be specified, relative to Enterprise");
            }

            string user = null;
            string password = null;

            // check the authentication type, and default to integrated if nothing sensible has been configured 
            if (_binding.Properties.ContainsKey(AuthTypePropertyKey))
            {
                var configuredAuthenticationType = (string) _binding.Properties[AuthTypePropertyKey];
                if (!string.IsNullOrEmpty(configuredAuthenticationType))
                {
                    if (configuredAuthenticationType.Equals("basic", StringComparison.CurrentCultureIgnoreCase))
                    {
                        LogVerbose(_log, "Setting current authentication method to basic...checking for user and password");
                        if (_binding.Properties.ContainsKey(UserPropertyKey))
                        {
                            user = (string) _binding.Properties[UserPropertyKey];
                        }
                        else
                        {
                            LogWarning(_log, "Basic authentication set, but no user specified");
                        }

                        if (_binding.Properties.ContainsKey(PasswordPropertyKey))
                        {
                            password = (string) _binding.Properties[PasswordPropertyKey];
                        }
                        else
                        {
                            LogWarning(_log, "Basic authentication set, but no password specified");
                        }

                        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                        {
                            LogWarning(_log, "Missing user or password for basic authentication - may not authenticate successfully");
                        }

                        _client = new WebServiceClient(baseEndpointAddress, user!, password!);
                    }
                    else
                    {
                        LogVerbose(_log, "Setting authentication to \"integrated\"");
                        _client = new WebServiceClient(baseEndpointAddress);
                    }
                }
                else
                {
                    LogWarning(_log,
                        "No authentication type has been specified within the binding configuration, so defaulting to \"Integrated\"");
                    LogVerbose(_log, "Setting authentication to \"integrated\"");
                    _client = new WebServiceClient(baseEndpointAddress);
                }
            }
            else
            {
                LogWarning(_log,
                    "No authentication type has been specified within the binding configuration, so defaulting to \"Integrated\"");
                LogVerbose(_log, "Setting authentication to \"integrated\"");
                _client = new WebServiceClient(baseEndpointAddress);
            }
        }
    }
}
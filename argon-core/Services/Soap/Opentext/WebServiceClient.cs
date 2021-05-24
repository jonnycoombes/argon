using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Services.Soap.Opentext
{
    /// <summary>
    ///     This class encapsulates the logic to bind to OTCS Content Web Services.  It is intended to be used by any classes/methods that need
    ///     to authenticate against various CWS services and then retrieve the underlying service interfaces
    /// </summary>
    public class WebServiceClient
    {
        /// <summary>
        ///     The authentication type to be used. Two are supported, basic authentication (with username and password) or integrated
        ///     if the CWS endpoints are configured for IWA-based authentication
        /// </summary>
        public enum AuthenticationType
        {
            Basic,
            Integrated
        }

        /// <summary>
        ///     Used for generic exception reporting
        /// </summary>
        public const string SOAPErrorMessage = "SOAP fault caught whilst making CWS outcall";

        /// <summary>
        ///     The authentication service suffix
        /// </summary>
        private const string AuthenticationServiceSuffix = "Authentication.svc";

        /// <summary>
        ///     The document management service suffix
        /// </summary>
        private const string DocumentManagementServiceSuffix = "DocumentManagement.svc";

        /// <summary>
        ///     Static logger for this class
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<WebServiceClient>();

        /// <summary>
        ///     Mutex used for preventing unsynchronised updates to things such as the current authentication token
        /// </summary>
        private readonly Mutex _stateMutex = new Mutex();

        /// <summary>
        ///     Used to cache groups of services against a specific endpoint base address
        /// </summary>
        private readonly Dictionary<string, EndpointServices> endpointServiceCache = new Dictionary<string, EndpointServices>();

        /// <summary>
        ///     Backing field for the current OT authentication token
        /// </summary>
        private OTAuthentication? _currentAuthentication;

        /// <summary>
        ///     Instantiates an instance of the binder and configures it for <see cref="AuthenticationType.Basic" /> authentication
        /// </summary>
        /// <param name="endpoint">The endpoint prefix for the services to be bound</param>
        /// <param name="username">The user name to be used</param>
        /// <param name="password">The password to be used</param>
        public WebServiceClient(string endpoint, string username, string password)
        {
            LogMethodCall(_log);
            LogVerbose(_log, $"Creating new client configured for basic authentication against \"{endpoint}\"");
            BaseEndpointAddress = PreconditionEndpoint(endpoint);
            User = username;
            Password = password;
            Authentication = AuthenticationType.Basic;
        }

        /// <summary>
        ///     Instantiates an instance of the binder and configures it for <see cref="AuthenticationType.Integrated" />
        ///     authentication
        /// </summary>
        /// <param name="endpoint">The endpoint prefix for the services to be bound</param>
        public WebServiceClient(string endpoint)
        {
            LogMethodCall(_log);
            LogVerbose(_log, $"Creating new client configured for integrated authentication against \"{endpoint}\"");
            BaseEndpointAddress = PreconditionEndpoint(endpoint);
            Authentication = AuthenticationType.Integrated;
        }

        /// <summary>
        ///     The current type of authentication to use.  The default is set to be integrated (IWA), so that no user or password information
        ///     is required within the binding information for the provider
        /// </summary>
        public AuthenticationType Authentication { get; set; }

        /// <summary>
        ///     The base endpoint address for the current bindings
        /// </summary>
        public string BaseEndpointAddress { get; set; }

        /// <summary>
        ///     The current user name (may be null
        /// </summary>
        private string? User { get; }

        /// <summary>
        ///     The current password
        /// </summary>
        private string? Password { get; }

        /// <summary>
        ///     Late-bound accessor for an instance of <see cref="DocumentManagement" />
        /// </summary>
        private DocumentManagement DocumentManagementService => ResolveEndpointServices().DocumentManagement;

        /// <summary>
        ///     Late-bound accessor for an instance of <see cref="Authentication" />
        /// </summary>
        private Authentication AuthenticationService => ResolveEndpointServices().Authentication;

        /// <summary>
        ///     Just makes sure that there's a trailing slash on the end of the specified endpoint
        /// </summary>
        /// <param name="endpoint">The raw endpoint</param>
        /// <returns></returns>
        private static string PreconditionEndpoint(string endpoint)
        {
            if (!endpoint.EndsWith("/"))
            {
                endpoint = $"{endpoint}/";
            }

            return endpoint;
        }

        /// <summary>
        ///     Generates a full endpoint address, based on a given base address and a specified service type
        /// </summary>
        /// <param name="baseAddress">The base address to use (should be a valid URI in it's own right)</param>
        /// <param name="serviceType">An element taken from the <see cref="ServiceType" /> enumeration</param>
        /// <returns>A complete endpoint address which may then be passed through to service client creation constructors</returns>
        private static string GenerateEndpointAddress(string baseAddress, ServiceType serviceType)
        {
            LogMethodCall(_log);
            var sanitisedBaseAddress = baseAddress;
            return serviceType switch
            {
                ServiceType.DocumentManagementService => $"{sanitisedBaseAddress}{DocumentManagementServiceSuffix}",
                _ => $"{sanitisedBaseAddress}{AuthenticationServiceSuffix}"
            };
        }


        /// <summary>
        ///     Attempts to resolve a set of bound services.   If the services haven't already been created and cached, they are bound
        ///     and then returned.  Sets of service implementations are cached against a base endpoint address
        /// </summary>
        /// <returns>A new populated <see cref="EndpointServices" /> instance</returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException">Thrown if an error occurs during service binding</exception>
        private EndpointServices ResolveEndpointServices()
        {
            LogMethodCall(_log);
            lock (endpointServiceCache)
            {
                try
                {
                    var scheme = new Uri(BaseEndpointAddress).Scheme;
                    if (!endpointServiceCache.ContainsKey(BaseEndpointAddress))
                    {
                        LogVerbose(_log, $"No bound services found for base endpoint address \"{BaseEndpointAddress}\" - binding");
                        var endpointServices = new EndpointServices();
                        switch (scheme)
                        {
                            case "https":
                                endpointServices.Authentication = new AuthenticationClient(
                                    AuthenticationClient.EndpointConfiguration.BasicHttpsBinding_Authentication,
                                    GenerateEndpointAddress(BaseEndpointAddress, ServiceType.AuthenticationService));
                                endpointServices.DocumentManagement = new DocumentManagementClient(
                                    DocumentManagementClient.EndpointConfiguration.BasicHttpsBinding_DocumentManagement,
                                    GenerateEndpointAddress(BaseEndpointAddress, ServiceType.DocumentManagementService));
                                break;
                            default:
                                endpointServices.Authentication = new AuthenticationClient(
                                    AuthenticationClient.EndpointConfiguration.BasicHttpBinding_Authentication,
                                    GenerateEndpointAddress(BaseEndpointAddress, ServiceType.AuthenticationService));
                                endpointServices.DocumentManagement = new DocumentManagementClient(
                                    DocumentManagementClient.EndpointConfiguration.BasicHttpBinding_DocumentManagement,
                                    GenerateEndpointAddress(BaseEndpointAddress, ServiceType.DocumentManagementService));
                                break;
                        }

                        endpointServiceCache.Add(BaseEndpointAddress, endpointServices);
                        return endpointServices;
                    }

                    LogVerbose(_log, $"Using pre-cached bound services for base endpoint address \"{BaseEndpointAddress}\"");
                    return endpointServiceCache[BaseEndpointAddress];
                }
                catch (Exception ex)
                {
                    LogWarning(_log, "Caught an exception whilst attempting to bind a new set of services");
                    LogExceptionWarning(_log, ex);
                    throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status500InternalServerError,
                        $"Exception whilst attempting to bind services \"{ex.Message}\" - check logs for more information");
                }
            }
        }

        /// <summary>
        ///     Performs authentication using the CWS authentication service, and returns a newly constructed
        ///     <see cref="OTAuthentication" /> instance
        /// </summary>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException">Thrown if the authentication fails for any reason</exception>
        private async Task Authenticate()
        {
            try
            {
                if (_currentAuthentication == null)
                {
                    LogVerbose(_log, "Not currently authenticated, performing authentication");
                    string token;
                    switch (Authentication)
                    {
                        case AuthenticationType.Basic:
                            token = await AuthenticationService.AuthenticateUserAsync(User, Password);
                            break;
                        default:
                            token = await AuthenticationService.AuthenticateUserAsync(null, null);
                            break;
                    }

                    UpdateAuthenticationToken(new OTAuthentication {AuthenticationToken = token});
                }

                LogVerbose(_log, "Currently authenticated, so using cached credentials");
            }
            catch (Exception ex)
            {
                throw new WebServiceClientException($"Unexpected exception during OTCS authentication: {ex.Message}");
            }
        }

        /// <summary>
        ///     Synchronised (mutex'd) method for updating the current <see cref="OTAuthentication" /> instance
        /// </summary>
        /// <param name="authentication">An <see cref="OTAuthentication" /> instance</param>
        private void UpdateAuthenticationToken(OTAuthentication authentication)
        {
            _stateMutex.WaitOne();
            _currentAuthentication = authentication;
            _stateMutex.ReleaseMutex();
        }

        /// <summary>
        ///     Wrapper around the CWS ListNodes operation.  Just retrieves *one* level of child nodes
        /// </summary>
        /// <param name="parentId">The id for the parent node</param>
        /// <returns>An array of child <see cref="Node" /> instances</returns>
        /// <exception cref="WebServiceClientException">Thrown in the event of a fault</exception>
        public async Task<Node[]> GetChildren(long parentId)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.ListNodesAsync(new ListNodesRequest
                {
                    OTAuthentication = _currentAuthentication,
                    parentID = parentId
                });
                UpdateAuthenticationToken(response.OTAuthentication);
                return response.ListNodesResult;
            }
            catch (Exception ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Wrapper around the CWS GetNode operation
        /// </summary>
        /// <param name="nodeId">The node id of the <see cref="Node" /> to be retrieved</param>
        /// <returns>A <see cref="Node" /> instance</returns>
        /// <exception cref="WebServiceClientException">Thrown if a SOAP fault is caught</exception>
        public async Task<Node> GetNode(long nodeId)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.GetNodeAsync(new GetNodeRequest
                {
                    OTAuthentication = _currentAuthentication,
                    ID = nodeId
                });
                UpdateAuthenticationToken(response.OTAuthentication);
                return response.GetNodeResult;
            }
            catch (Exception ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Wrapper around the CWS GetNodeByPath operation
        /// </summary>
        /// <param name="rootId">The root node id to start from</param>
        /// <param name="elements">An array of path elements</param>
        /// <returns>A <see cref="Node" /> instance</returns>
        /// <exception cref="WebServiceClientException"></exception>
        public async Task<Node?> GetNodeByPath(long rootId, string[] elements)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.GetNodeByPathAsync(new GetNodeByPathRequest
                {
                    OTAuthentication = _currentAuthentication,
                    rootID = rootId,
                    pathElements = elements
                });
                UpdateAuthenticationToken(response.OTAuthentication);
                return response.GetNodeByPathResult;
            }
            catch (Exception ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        public async Task<String> GetItemVersion(long nodeId, long version)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.GetVersionAsync(new GetVersionRequest
                {
                    OTAuthentication = _currentAuthentication,
                    ID = nodeId,
                    versionNum = version
                });
                UpdateAuthenticationToken(response.OTAuthentication);
                return response.GetStringResult;
            }
            catch (Exception ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Wrapper around the CWS GetNodeByName operation
        /// </summary>
        /// <param name="parentId">The parent node id</param>
        /// <param name="name">The name of the node to retrieve</param>
        /// <returns>A <see cref="Node" /> instance or null if no such node exists</returns>
        /// <exception cref="WebServiceClientException"></exception>
        public async Task<Node?> GetNodeByName(long parentId, string name)
        {
            try
            {
                await Authenticate();
                var response =
                    await DocumentManagementService.GetNodeByNameAsync(new GetNodeByNameRequest
                    {
                        OTAuthentication = _currentAuthentication,
                        parentID = parentId,
                        name = name
                    });
                UpdateAuthenticationToken(response.OTAuthentication);
                return response.GetNodeByNameResult;
            }
            catch (Exception ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Wrapper around the CWS CreateFolder operation
        /// </summary>
        /// <param name="parentId">The parent node id</param>
        /// <param name="name">The name of the folder to create</param>
        /// <param name="comments">The comments associated with the new folder</param>
        /// <param name="metadata">The meta-data associated with the new folder</param>
        /// <returns>A <see cref="Node" /> instance corresponding to the new folder</returns>
        /// <exception cref="WebServiceClientException"></exception>
        public async Task<Node?> CreateFolder(long parentId, string name, string? comments = "Created by Argon", Metadata? metadata = null)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.CreateFolderAsync(new CreateFolderRequest
                {
                    parentID = parentId,
                    OTAuthentication = _currentAuthentication,
                    name = name,
                    comment = comments,
                    metadata = metadata
                });
                UpdateAuthenticationToken(response.OTAuthentication);
                return response.CreateFolderResult;
            }
            catch (Exception ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Wrapper around the CWS DeleteNode operation
        /// </summary>
        /// <param name="nodeId"></param>
        /// <exception cref="WebServiceClientException"></exception>
        public async Task DeleteNode(long nodeId)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.DeleteNodeAsync(new DeleteNodeRequest(_currentAuthentication, nodeId));
                UpdateAuthenticationToken(response.OTAuthentication);
            }
            catch (FaultException ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Wrapper around the CWS CreateDocument operation
        /// </summary>
        /// <param name="parentId">The parent id of the container to create the document in</param>
        /// <param name="filename">The filename (name) for the new document</param>
        /// <param name="attachment">The <see cref="Attachment" /> instance containing the actual contents</param>
        /// <param name="comments">Optional comments</param>
        /// <param name="advancedVersionControl">Whether or not advanced version control should be used</param>
        /// <param name="metadata">Optional <see cref="Metadata" /> structure for the new document</param>
        /// <returns>A new <see cref="Node" /> instance</returns>
        /// <exception cref="WebServiceClientException"></exception>
        public async Task<Node> CreateDocument(long parentId, string filename, Attachment attachment, string? comments = "Created by Argon",
            bool
                advancedVersionControl = false, Metadata metadata = null)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.CreateDocumentAsync(new CreateDocumentRequest
                {
                    OTAuthentication = _currentAuthentication,
                    parentID = parentId,
                    advancedVersionControl = advancedVersionControl,
                    attach = attachment,
                    comment = comments,
                    name = filename,
                    metadata = metadata
                });
                UpdateAuthenticationToken(response.OTAuthentication);
                return response.CreateDocumentResult;
            }
            catch (WebServiceClientException ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Wrapper around the CWS AddVersion operation
        /// </summary>
        /// <param name="documentId">The id of an existing document</param>
        /// <param name="attachment">The actual <see cref="Attachment" /> containing the contents of the new version</param>
        /// <param name="metadata">Optional <see cref="Metadata" /> structure</param>
        /// <returns>A new <see cref="String" /> instance</returns>
        /// <exception cref="WebServiceClientException"></exception>
        public async Task<String> AddVersion(long documentId, Attachment attachment, Metadata metadata = null)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.AddVersionAsync(new AddVersionRequest
                {
                    OTAuthentication = _currentAuthentication,
                    attach = attachment,
                    metadata = metadata
                });
                UpdateAuthenticationToken(response.OTAuthentication);
                return response.AddStringResult;
            }
            catch (WebServiceClientException ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Wrapper around the CWS GetVersionContents operation
        /// </summary>
        /// <param name="documentId">The id for the document</param>
        /// <param name="versionNumber">The version number to retrieve</param>
        /// <returns>An <see cref="Attachment" /> instance containing the contents of the version</returns>
        /// <exception cref="WebServiceClientException"></exception>
        public async Task<Attachment> GetVersionContents(long documentId, long versionNumber)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.GetVersionContentsAsync(new GetVersionContentsRequest
                {
                    OTAuthentication = _currentAuthentication,
                    ID = documentId,
                    versionNum = versionNumber
                });
                UpdateAuthenticationToken(response.OTAuthentication);
                return response.GetVersionContentsResult;
            }
            catch (WebServiceClientException ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Wrapper around the CWS UpdateNode operation
        /// </summary>
        /// <param name="node">The <see cref="Node" /> including all its updates</param>
        /// <exception cref="WebServiceClientException"></exception>
        public async Task UpdateNode(Node node)
        {
            try
            {
                await Authenticate();
                var response = await DocumentManagementService.UpdateNodeAsync(new UpdateNodeRequest
                {
                    OTAuthentication = _currentAuthentication,
                    node = node
                });
                UpdateAuthenticationToken(response.OTAuthentication);
            }
            catch (FaultException ex)
            {
                throw new WebServiceClientException(SOAPErrorMessage, ex);
            }
        }

        /// <summary>
        ///     Placeholder class which is used to statically cache a group of services (which are expensive to create) against a specific
        ///     endpoint base address
        /// </summary>
        private class EndpointServices
        {
            /// <summary>
            ///     The <see cref="Authentication" /> service
            /// </summary>
            public Authentication Authentication { get; set; }

            /// <summary>
            ///     The <see cref="DocumentManagement" /> service
            /// </summary>
            public DocumentManagement DocumentManagement { get; set; }
        }

        /// <summary>
        ///     An enumeration of the possible SOAP service endpoints (used to construct new endpoint addresses from a base endpoint address)
        /// </summary>
        private enum ServiceType
        {
            AuthenticationService,
            DocumentManagementService
        }
    }
}
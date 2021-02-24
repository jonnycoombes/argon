using System;
using System.Collections.Generic;
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
        ///     The authentication type to be used. Two are supported, basic authentication (with username and password) or integrated
        ///     if the CWS endpoints are configured for IWA-based authentication
        /// </summary>
        public enum AuthenticationType
        {
            Basic,
            Integrated
        }

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
        ///     The authentication service suffix
        /// </summary>
        private const string AuthenticationServiceSuffix = "Authentication.svc";

        /// <summary>
        ///     The document management service suffix
        /// </summary>
        private const string DocumentManagementServiceSuffix = "DocumentManagment.svc";

        /// <summary>
        ///     The content service suffix
        /// </summary>
        private const string ContentServiceSuffix = "ContentService.svc";

        /// <summary>
        ///     The admin service suffix
        /// </summary>
        private const string AdminServiceSuffix = "AdminService.svc";

        /// <summary>
        ///     The config service suffix
        /// </summary>
        private const string ConfigServiceSuffix = "ConfigService.svc";

        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<OtcsSoapStorageProvider>();

        /// <summary>
        ///     Used to cache groups of services against a specific endpoint base address
        /// </summary>
        private static readonly Dictionary<string, EndpointServices> endpointServiceCache = new();

        /// <summary>
        ///     The current type of authentication to use.  The default is set to be integrated (IWA), so that no user or password information
        ///     is required within the binding information for the provider
        /// </summary>
        private AuthenticationType authenticationType = AuthenticationType.Integrated;

        /// <summary>
        ///     The current base endpoint address
        /// </summary>
        private string baseEndpointAddress;

        /// <summary>
        ///     The current password (basic authentication only)
        /// </summary>
        private string password;

        /// <summary>
        ///     The root path.  All collections are stored underneath this location relative to the Enterprise workspace within OTCS
        /// </summary>
        private string rootCollectionPath;

        /// <summary>
        ///     The current user name (basic authentication only)
        /// </summary>
        private string userName;

        /// <summary>
        ///     The provider type tag for this type of provider - should correlate to settings within the appsettings.json file
        /// </summary>
        public override string ProviderType => "openTextSoap";

        /// <summary>
        ///     Attempts to resolve a set of bound services.   If the services haven't already been created and cached, they are bound
        ///     and then returned.  Sets of service implementations are cached against a base endpoint address
        /// </summary>
        /// <param name="baseAddress">The base address, which acts as a key into the cache</param>
        /// <returns>A new populated <see cref="EndpointServices" /> instance</returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException">Thrown if an error occurs during service binding</exception>
        private static EndpointServices ResolveEndpointServices(string baseAddress)
        {
            LogMethodCall(_log);
            lock (endpointServiceCache)
            {
                try
                {
                    if (!endpointServiceCache.ContainsKey(baseAddress))
                    {
                        LogVerbose(_log, $"No bound services found for base endpoint address \"{baseAddress}\" - binding");
                        var endpointServices = new EndpointServices
                        {
                            Authentication =
                                new AuthenticationClient(AuthenticationClient.EndpointConfiguration.BasicHttpBinding_Authentication,
                                    GenerateEndpointAddress(baseAddress, ServiceType.AuthenticationService)),
                            DocumentManagement =
                                new DocumentManagementClient(
                                    DocumentManagementClient.EndpointConfiguration.BasicHttpBinding_DocumentManagement,
                                    GenerateEndpointAddress(baseAddress, ServiceType.DocumentManagementService)),
                            AdminService =
                                new AdminServiceClient(AdminServiceClient.EndpointConfiguration.BasicHttpBinding_AdminService,
                                    GenerateEndpointAddress(baseAddress, ServiceType.AdminService)),
                            ContentService =
                                new ContentServiceClient(ContentServiceClient.EndpointConfiguration.BasicHttpBinding_ContentService,
                                    GenerateEndpointAddress(baseAddress, ServiceType.ContentService)),
                            ConfigService =
                                new ConfigServiceClient(ConfigServiceClient.EndpointConfiguration.BasicHttpBinding_ConfigService,
                                    GenerateEndpointAddress(baseAddress, ServiceType.ConfigService))
                        };
                        endpointServiceCache.Add(baseAddress, endpointServices);
                        return endpointServices;
                    }

                    LogVerbose(_log, $"Using pre-cached bound services for base endpoint address \"{baseAddress}\"");
                    return endpointServiceCache[baseAddress];
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
                ServiceType.AdminService => $"{sanitisedBaseAddress}{AdminServiceSuffix}",
                ServiceType.ConfigService => $"{sanitisedBaseAddress}{ConfigServiceSuffix}",
                ServiceType.ContentService => $"{sanitisedBaseAddress}{ContentServiceSuffix}",
                _ => $"{sanitisedBaseAddress}{AuthenticationServiceSuffix}"
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="collection">The <see cref="Collection" /> instance to create the underlying collection location/folder for</param>
        /// <returns>A valid <see cref="IVirtualStorageProvider.StorageOperationResult" /></returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException"></exception>
        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            LogMethodCall(_log);

            var authenticationToken = await Authenticate();

            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="itemVersion"></param>
        /// <param name="source"></param>
        /// <returns>A valid <see cref="IVirtualStorageProvider.StorageOperationResult" /></returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException"></exception>
        public override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection,
            Item item, ItemVersion itemVersion, IFormFile source)
        {
            LogMethodCall(_log);
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <returns>A valid <see cref="IVirtualStorageProvider.StorageOperationResult" /></returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException"></exception>
        public override Task<IVirtualStorageProvider.StorageOperationResult> DeleteCollectionItemAsync(Collection collection, Item item)
        {
            LogMethodCall(_log);
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Performs authentication using the CWS authentication service, and caches the authentication token material for subsequent calls
        /// </summary>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException">Thrown if the authentication fails for any reason</exception>
        public async Task<OTAuthentication> Authenticate()
        {
            var services = ResolveEndpointServices(baseEndpointAddress);
            try
            {
                var token = await services.Authentication.AuthenticateUserAsync(userName, password);
                return new OTAuthentication {AuthenticationToken = token};
            }
            catch (FaultException ex)
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status401Unauthorized,
                    $"OTCS Fault exception: {ex.Message},{ex.Code}");
            }
            catch (Exception ex)
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(StatusCodes.Status401Unauthorized,
                    $"Unexpected exception during OTCS authentication: {ex.Message}");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="itemVersion"></param>
        /// <returns>A valid <see cref="IVirtualStorageProvider.StorageOperationResult" /></returns>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException"></exception>
        public override Task<IVirtualStorageProvider.StorageOperationResult> ReadCollectionItemVersionAsync(Collection collection,
            Item item, ItemVersion itemVersion)
        {
            LogMethodCall(_log);
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Checks the current <see cref="JCS.Argon.Model.Configuration.VirtualStorageBinding" /> and then synthesises any additional configuration
        ///     information that may be required, such as the complete service endpoints.
        /// </summary>
        /// <exception cref="IVirtualStorageProvider.VirtualStorageProviderException">Thrown in the case of an invalid configuration</exception>
        public override void AfterBind()
        {
            LogMethodCall(_log);

            // check that we have a valid base endpoint address
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
                            userName = (string) _binding.Properties[UserPropertyKey];
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

                        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                        {
                            LogWarning(_log, "Missing user or password for basic authentication - may not authenticate successfully");
                        }
                    }
                    else
                    {
                        authenticationType = AuthenticationType.Integrated;
                        LogVerbose(_log, "Setting authentication to \"integrated\"");
                    }
                }
                else
                {
                    LogWarning(_log,
                        "No authentication type has been specified within the binding configuration, so defaulting to \"Integrated\"");
                    authenticationType = AuthenticationType.Integrated;
                    LogVerbose(_log, "Setting authentication to \"integrated\"");
                }
            }
            else
            {
                LogWarning(_log,
                    "No authentication type has been specified within the binding configuration, so defaulting to \"Integrated\"");
                authenticationType = AuthenticationType.Integrated;
                LogVerbose(_log, "Setting authentication to \"integrated\"");
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

            /// <summary>
            ///     The <see cref="ContentService" /> service
            /// </summary>
            public ContentService ContentService { get; set; }

            /// <summary>
            ///     The <see cref="AdminService" /> service
            /// </summary>
            public AdminService AdminService { get; set; }

            /// <summary>
            ///     The <see cref="ConfigService" /> service
            /// </summary>
            public ConfigService ConfigService { get; set; }
        }

        /// <summary>
        ///     An enumeration of the possible SOAP service endpoints (used to construct new endpoint addresses from a base endpoint address)
        /// </summary>
        private enum ServiceType
        {
            AuthenticationService,
            DocumentManagementService,
            ContentService,
            AdminService,
            ConfigService
        }
    }
}
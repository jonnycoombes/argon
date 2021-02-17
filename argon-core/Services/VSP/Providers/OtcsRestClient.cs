﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Core;
using JCS.Argon.Utility;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;
using static JCS.Neon.Glow.Helpers.General.ParseHelpers;

#endregion

namespace JCS.Argon.Services.VSP.Providers
{
    public class OtcsRestClient : BaseRestClient
    {
        /// <summary>
        ///     The authentication type to be used - i.e. whether or not we embed a token into any requests sent to the server
        /// </summary>
        public enum AuthenticationType
        {
            Basic,
            Integrated
        }

        /// <summary>
        ///     The expected OT authentication header field
        /// </summary>
        private const string OtcsTicketHeader = "OTCSTicket";

        /// <summary>
        ///     This never changes between CS instances
        /// </summary>
        private const int EnterpriseNodeId = 2000;

        /// <summary>
        ///     The REST api authentication endpoint suffix
        /// </summary>
        private const string AuthEndpointSuffix = "api/v1/auth";

        /// <summary>
        ///     The v2 nodes api suffix
        /// </summary>
        private const string NodesV2Suffix = "api/v2/nodes";

        /// <summary>
        ///     The v1 nodes api suffix
        /// </summary>
        private const string NodesV1Suffix = "api/v1/nodes";

        /// <summary>
        ///     A suffix which is appended to the endpoint address in order to try and extract a LLCookie value or ticket from a pass-thru
        ///     authentication request
        /// </summary>
        private const string SmartViewEnterpriseSuffix = "app/nodes/2000";

        /// <summary>
        ///     The name of the Otcs cookie to look for
        /// </summary>
        private const string OtcsCookieName = "LLCookie";

        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<OtcsRestClient>();

        /// <summary>
        /// </summary>
        /// <param name="cache"></param>
        public OtcsRestClient(IDbCache? cache)
        {
            LogMethodCall(_log);
            Cache = cache;
        }

        /// <summary>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="cache"></param>
        /// <param name="cachePartition"></param>
        /// <param name="endpointAddress"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public OtcsRestClient(ILogger log, IDbCache cache, string cachePartition,
            string endpointAddress, string userName, string password)
        {
            LogMethodCall(_log);
            CachePartition = cachePartition;
            Cache = cache;
            EndpointAddress = endpointAddress.EndsWith('/') ? endpointAddress : $"{endpointAddress}/";
            UserName = userName;
            Password = password;
        }

        /// <summary>
        ///     Constructs a REST client that utilises built-in network connections
        /// </summary>
        /// <param name="log"></param>
        /// <param name="cache"></param>
        /// <param name="cachePartition"></param>
        /// <param name="endpointAddress"></param>
        public OtcsRestClient(ILogger log, IDbCache cache, string cachePartition, string endpointAddress)
        {
            LogMethodCall(_log);
            CachePartition = cachePartition;
            Cache = cache;
            EndpointAddress = endpointAddress.EndsWith('/') ? endpointAddress : $"{endpointAddress}/";
            OtcsAuthenticationType = AuthenticationType.Integrated;
        }

        /// <summary>
        ///     The currently configured authentication type to be used against Otcs
        /// </summary>
        public AuthenticationType OtcsAuthenticationType { get; set; } = AuthenticationType.Basic;

        /// <summary>
        ///     An optional instance of <see cref="IDbCache" /> which may be used for stashing
        ///     useful information.  (Mainly node ids).
        /// </summary>
        private IDbCache? Cache { get; }

        /// <summary>
        ///     The current REST base endpoint address
        /// </summary>
        /// <value></value>
        public string? EndpointAddress { get; set; }

        /// <summary>
        ///     The user to be used within basic authentication requests
        /// </summary>
        /// <value></value>
        public string? UserName { get; set; }

        /// <summary>
        ///     The password to be used for basic authentication
        /// </summary>
        /// <value></value>
        public string? Password { get; set; }

        /// <summary>
        ///     The current authentication token
        /// </summary>
        /// <value></value>
        private string? AuthenticationToken { get; set; }

        /// <summary>
        ///     A partition value to use with the supplied instance of <see cref="IDbCache" />
        /// </summary>
        /// <value></value>
        public string CachePartition { get; set; } = null!;

        /// <summary>
        ///     Validate the current configuration
        /// </summary>
        /// <exception cref="OpenTextRestClientException">
        ///     This will be thrown if we don't currently have a valid configuration.  Will result in a 500
        ///     error code being returned to the client
        /// </exception>
        private void ValidateConfiguration()
        {
            LogMethodCall(_log);
            var validated = false;
            if (OtcsAuthenticationType == AuthenticationType.Basic)
            {
                if (!string.IsNullOrEmpty(EndpointAddress) && ParseUri(EndpointAddress).IsSome()
                                                           && !string.IsNullOrEmpty(UserName)
                                                           && !string.IsNullOrEmpty(Password))
                {
                    validated = true;
                }
            }

            if (!string.IsNullOrEmpty(EndpointAddress) && ParseUri(EndpointAddress).IsSome())
            {
                validated = true;
            }

            // just make sure that we have a trailing slash on the endpoint address
            if (validated)
            {
                EndpointAddress = EndpointAddress.EndsWith('/') ? EndpointAddress : $"{EndpointAddress}/";
            }

            if (!validated)
            {
                LogWarning(_log, $"{GetType()}: Failed to validate current configuration");
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    "OpenText REST Client is not currently configured correctly");
            }
        }

        /// <summary>
        ///     Attempts an authentication operation and stashes away the authentication ticket/token
        /// </summary>
        /// <exception cref="OpenTextRestClientException"></exception>
        public async Task Authenticate()
        {
            LogMethodCall(_log);
            ValidateConfiguration();

            switch (OtcsAuthenticationType)
            {
                case AuthenticationType.Integrated:
                {
                    AuthenticationToken = await PerformIntegratedAuthentication();
                    break;
                }
                default:
                {
                    AuthenticationToken = await PerformBasicAuthentication();
                    break;
                }
            }
        }

        /// <summary>
        ///     Performs basic authentication using the Otcs REST authentication endpoint.  Note that this will still work in an integrated
        ///     environment, however you will need to supply a valid username and password
        /// </summary>
        /// <returns></returns>
        private async Task<string> PerformBasicAuthentication()
        {
            LogMethodCall(_log);
            string token;
            JObject json;

            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
            {
                throw new OpenTextRestClientException(StatusCodes.Status401Unauthorized,
                    "No credentials supplied for OTCS authentication, please check system configuration");
            }

            var content = CreateMultiPartFormTemplate(new[]
            {
                ("username", UserName!),
                ("password", Password!)
            });

            try
            {
                json = await PostMultiPartRequestForJsonAsync(new Uri($"{EndpointAddress}{AuthEndpointSuffix}"),
                    new (string, string)[] { }, content);
            }
            catch (Exception ex)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"An exception was caught during an OTCS out-call: \"{ex.GetBaseException().Message}\"", ex);
            }

            if (!json.ContainsKey("ticket"))
            {
                throw new OpenTextRestClientException(StatusCodes.Status401Unauthorized,
                    "Failed to authenticate against OTCS  - no ticket element returned in authentication response");
            }

            token = (string) json["ticket"]!;
            if (string.IsNullOrEmpty(token))
            {
                throw new OpenTextRestClientException(StatusCodes.Status401Unauthorized,
                    "Failed to authenticate against OTCS - empty or invalid token returned in authentication response");
            }


            return token;
        }

        /// <summary>
        ///     Attempts to perform integrated authentication by "sniffing" a valid authentication token from the Otcs endpoint.  Basically,
        ///     this method will issue a GET request against a 16.x "Smart UI" URL, and then try to extract a valid authentication token from
        ///     within the returned page body
        /// </summary>
        /// <returns></returns>
        private async Task<string> PerformIntegratedAuthentication()
        {
            LogMethodCall(_log);
            HttpResponseMessage response;
            string token = null;
            try
            {
                LogVerbose(_log, "Attempting integrated authentication into OTCS, using default network credentials");
                response = await GetRequest(new Uri($"{EndpointAddress}{SmartViewEnterpriseSuffix}"), null, null);
            }
            catch (Exception ex)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"Exception caught during OTCS integrated authentication request: \"{ex.Message}\"", ex);
            }

            LogVerbose(_log, "Attempting extraction of Otcs authentication ticket from integrated authentication response");
            var payload = await response.Content.ReadAsStringAsync();
            var lines = payload.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
            if (lines.Any(s => s.TrimStart().StartsWith("ticket:")))
            {
                token = lines.First(s => s.TrimStart().StartsWith("ticket:")).TrimStart().Split(":")[1].Replace("'", "");
            }
            else
            {
                LogError(_log, "Could not locate a valid OTCS ticket in integrated authentication response");
                throw new OpenTextRestClientException(StatusCodes.Status401Unauthorized,
                    "Failed to authenticate against OTCS, integrated response didn't contain valid ticket values");
            }

            return token;
        }

        /// <summary>
        ///     Uploads a file to a given node (hopefully a folder)
        /// </summary>
        /// <param name="parentId">The node id of the parent folder</param>
        /// <param name="name">The Otcs name property to be set</param>
        /// <param name="fileName">The underlying filename to be applied to the newly uploaded item</param>
        /// <param name="source">A <see cref="Stream" /> that can be read in order to obtain the contents of the new file</param>
        /// <returns>An awaitable which eventually returns the node id of the newly uploaded file</returns>
        /// <exception cref="OpenTextRestClientException">Thrown if something goes wrong with the upload operation</exception>
        public async Task<long> UploadFile(long parentId, string name, string fileName, Stream source)
        {
            LogMethodCall(_log);
            ValidateConfiguration();
            var content = CreateMultiPartFormTemplate(
                new[]
                {
                    ("type", "144"),
                    ("parent_id", parentId.ToString()),
                    ("name", name)
                }
            );
            content.Add(new StreamContent(source), "file", fileName);
            try
            {
                var json = await PostMultiPartRequestForJsonAsync(new Uri($"{EndpointAddress}{NodesV2Suffix}"), GenerateHeaders(),
                    content);
                if (!json.ContainsKey("results"))
                {
                    throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                        "An invalid response was returned by OpenText outcall - results element wasn't found");
                }

                var raw = json["results"]["data"]["properties"]["id"].ToString();
                return long.Parse(raw);
            }
            catch
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    "An invalid response was returned by OpenText out-call - results element wasn't found");
            }
        }

        /// <summary>
        ///     Generates the base header set for a new request.  Basically just injects the Otcs authentication header
        /// </summary>
        /// <returns>An array of string pairs, containing zero or more headers to be added to a request</returns>
        private IEnumerable<(string, string)> GenerateHeaders()
        {
            return new (string, string)[] {(OtcsTicketHeader, AuthenticationToken)};
        }

        /// <summary>
        ///     Creates a folder underneath a given parent folder
        /// </summary>
        /// <param name="parentId">The id of the parent folder</param>
        /// <param name="name">The name for the folder</param>
        /// <param name="description">An optional description for the folder</param>
        /// <returns></returns>
        public async Task<long> CreateFolder(long parentId, string name, string description = "Automatically generated by Argon")
        {
            LogMethodCall(_log);
            ValidateConfiguration();
            var content = CreateMultiPartFormTemplate(
                new[]
                {
                    ("type", 0.ToString()),
                    ("parent_id", parentId.ToString()),
                    ("name", name),
                    ("description", description)
                });
            try
            {
                var json = await PostMultiPartRequestForJsonAsync(new Uri($"{EndpointAddress}{NodesV2Suffix}"), GenerateHeaders(),
                    content);
                if (!json.ContainsKey("results"))
                {
                    throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                        "An invalid response was returned by OpenText outcall - results element wasn't found");
                }

                var raw = json["results"]["data"]["properties"]["id"].ToString();
                return long.Parse(raw);
            }
            catch (Exception ex)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"An exception was caught during an OpenText outcall: {ex.GetBaseException().Message}", ex);
            }
        }

        public async Task<bool> HasChildFolder(long parentId, string name)
        {
            LogMethodCall(_log);
            return await GetChildId(parentId, name) != 0;
        }

        /// <summary>
        ///     Retrieves the content for a given node
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <param name="versionId">The version id</param>
        /// <returns></returns>
        public async Task<Stream> GetNodeVersionContent(long nodeId, int versionId = -1)
        {
            LogMethodCall(_log);
            ValidateConfiguration();

            var uri = versionId == -1
                ? new Uri($"{EndpointAddress}/{NodesV1Suffix}/{nodeId}/content")
                : new Uri($"{EndpointAddress}/{NodesV1Suffix}/{nodeId}/versions/{versionId}/content");

            try
            {
                var response = await GetRequest(uri, GenerateHeaders(), null);
                return await response.Content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"An exception was caught during an OpenText outcall: {ex.GetBaseException().Message}", ex);
            }
        }

        /// <summary>
        ///     Attempts the deletion of a given node
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns></returns>
        /// <exception cref="OpenTextRestClientException">Thrown if we get an error from the underlying Otcs REST layer</exception>
        public async Task DeleteNode(long nodeId)
        {
            LogMethodCall(_log);
            ValidateConfiguration();
            var uri = new Uri($"{EndpointAddress}/{NodesV1Suffix}/{nodeId}");
            try
            {
                await DeleteRequest(uri, GenerateHeaders(), null);
            }
            catch (Exception ex)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"An exception was caught during an OpenText outcall: {ex.GetBaseException().Message}", ex);
            }
        }

        /// <summary>
        ///     Tries to retrieve the child folder id for a given folder
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<long> GetChildId(long parentId, string name)
        {
            LogMethodCall(_log);
            ValidateConfiguration();
            try
            {
                var uri = new Uri($"{EndpointAddress}/{NodesV2Suffix}/{parentId}/nodes");
                var json = await GetRequestForJsonAsync(uri, GenerateHeaders(), new (string, string)[]
                {
                    ("where_name", name)
                });

                if (!json.ContainsKey("results") || !json["results"].HasValues)
                {
                    return 0;
                }

                var node = json["results"][0];
                return long.Parse(node["data"]["properties"]["id"].ToString());
            }
            catch (Exception ex)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"An exception was caught during an OpenText outcall: {ex.GetBaseException().Message}", ex);
            }
        }

        /// <summary>
        ///     The format for cached path entries
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GeneratePathCacheId(string path)
        {
            LogMethodCall(_log);
            return $"path:{path}";
        }

        /// <summary>
        ///     Adds a path and it's node id to the cache
        /// </summary>
        /// <param name="path">The path to be cached</param>
        /// <param name="id">The corresponding node id</param>
        /// <returns></returns>
        public async Task<CacheEntry> AddCachedPathId(string path, long id)
        {
            LogMethodCall(_log);
            this.AssertNotNull(Cache, "No cache has been supplied!");
            return await Cache?.AddOrReplaceLongValueAsync(CachePartition, GeneratePathCacheId(path), id)!;
        }

        /// <summary>
        ///     Returns the cache value associated with a given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<long> GetCachedPathId(string path)
        {
            LogMethodCall(_log);
            if (Cache == null) return 0;
            var key = GeneratePathCacheId(path);
            if (!await Cache.HasEntry(CachePartition, key)) return 0;
            var entry = await Cache.LookupEntry(CachePartition, key);
            return entry?.LongValue ?? 0;
        }

        /// <summary>
        ///     Attempts to create a path relative to the Enterprise node.  This method is cache-aware
        ///     based on a non-null <see cref="IDbCache" /> instance being provided and a partition.
        /// </summary>
        /// <param name="path">A path of the format "/folder1/folder2../folderN</param>
        /// <param name="cache"></param>
        /// <returns>The node id</returns>
        /// <exception cref="OtcsRestClient">Thrown if unable to create the path</exception>
        public async Task<long?> CreatePath(string path, bool cache = true)
        {
            var cacheId = await GetCachedPathId(path);
            if (cacheId != 0) return cacheId;
            long parentId = EnterpriseNodeId;
            var elements = path.Split('/');
            foreach (var element in elements)
            {
                var childId = await GetChildId(parentId, element);
                if (childId == 0)
                    parentId = await CreateFolder(parentId, element, $"Argon Managed Collection ({element})");
                else
                    parentId = childId;
            }

            if (cache)
                await Cache.AddOrReplaceLongValueAsync(CachePartition, GeneratePathCacheId(path), parentId);

            return parentId;
        }

        /// <summary>
        ///     Thrown if operations within the client fail
        /// </summary>
        public sealed class OpenTextRestClientException : ResponseAwareException
        {
            public OpenTextRestClientException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(OtcsRestClient);
            }

            public OpenTextRestClientException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(OtcsRestClient);
            }
        }
    }
}
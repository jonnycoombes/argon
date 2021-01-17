using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Core;
using JCS.Argon.Utility;
using Microsoft.AspNetCore.Http;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Services.VSP.Providers
{
    public class OpenTextRestClient : BaseRestClient
    {
        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext<OpenTextRestClient>();

        /// <summary>
        /// The expected OT authentication header field
        /// </summary>
        private const string OtcsticketHeader = "OTCSTicket";

        /// <summary>
        /// Thrown if operations within the client fail
        /// </summary>
        public sealed class OpenTextRestClientException : ResponseAwareException
        {
            public OpenTextRestClientException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(OpenTextRestClient);
            }

            public OpenTextRestClientException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(OpenTextRestClient);
            }
        }

        /// <summary>
        /// This never changes between CS instances
        /// </summary>
        public const int EnterpriseNodeId = 2000;

        /// <summary>
        /// The REST api authentication endpoint suffix
        /// </summary>
        public const string AuthEndpointSuffix = "v1/auth";

        /// <summary>
        /// The v2 nodes api suffix
        /// </summary>
        public const string NodesSuffix = "v2/nodes";

        /// <summary>
        /// An optional instance of <see cref="IDbCache" /> which may be used for stashing
        /// useful information.  (Mainly node ids).
        /// </summary>
        private IDbCache? Cache { get; set; }

        /// <summary>
        /// The current REST base endpoint address
        /// </summary>
        /// <value></value>
        public string? EndpointAddress { get; set; } = null!;

        /// <summary>
        /// The user to be used within basic authentication requests
        /// </summary>
        /// <value></value>
        public string? UserName { get; set; } = null!;

        /// <summary>
        /// The password to be used for basic authentication 
        /// </summary>
        /// <value></value>
        public string? Password { get; set; } = null!;

        /// <summary>
        /// The current authentication token
        /// </summary>
        /// <value></value>
        public string? AuthenticationToken { get; set; } = null!;

        /// <summary>
        /// A partition value to use with the supplied instance of <see cref="IDbCache" />
        /// </summary>
        /// <value></value>
        public string CachePartition { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        public OpenTextRestClient(IDbCache? cache) : base()
        {
            LogMethodCall(_log);
            Cache = cache;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="cache"></param>
        /// <param name="cachePartition"></param>
        /// <param name="endpointAddress"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public OpenTextRestClient(ILogger log, IDbCache cache, string cachePartition,
            string endpointAddress, string userName, string password) : base()
        {
            CachePartition = cachePartition;
            Cache = cache;
            EndpointAddress = endpointAddress.EndsWith('/') ? endpointAddress : $"{endpointAddress}/";
            UserName = userName;
            Password = password;
        }

        /// <summary>
        /// Just check the current configuration
        /// </summary>
        /// <exception cref="OpenTextRestClientException"></exception>
        protected void ValidateConfiguration()
        {
            if (EndpointAddress == null || UserName == null || Password == null)
            {
                LogWarning(_log,$"{this.GetType()}: Failed to validate current configuration");
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"OpenText REST Client is not currently configured correctly");
            }
        }

        /// <summary>
        /// Attempts an authentication operation and stashes away the authentication ticket/token
        /// </summary>
        /// <exception cref="OpenTextRestClientException"></exception>
        public async Task<string> Authenticate()
        {
            LogDebug(_log,$"{this.GetType()}: Attempting authentication");
            ValidateConfiguration();
            var content = CreateMultiPartFormTemplate(new (string, string)[]
            {
                ("username", UserName!),
                ("password", Password!)
            });

            try
            {
                var json = await PostMultiPartRequestForJsonAsync(new Uri($"{EndpointAddress}{AuthEndpointSuffix}"),
                    new (string, string)[] { }, content);
                if (json.ContainsKey("ticket"))
                {
                    AuthenticationToken = (string) json["ticket"]!;
                    LogDebug(_log,$"{this.GetType()}: Authentication successful");
                    return AuthenticationToken;
                }
                else
                {
                    throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                        $"Couldn't locate authentication ticket in OpenText response");
                }
            }
            catch (Exception ex)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"An exception was caught during an OpenText outcall: {ex.GetBaseException().Message}", ex);
            }
        }

        public async Task<long> UploadFile(long parentId, string name, string fileName, Stream source)
        {
            LogDebug(_log,$"{this.GetType()}: Attempting file upload");
            ValidateConfiguration();
            var content = CreateMultiPartFormTemplate(
                new (string, string)[]
                {
                    ("type", "144"),
                    ("parent_id", parentId.ToString()),
                    ("name", name)
                }
            );
            content.Add(new StreamContent(source), "file", fileName);
            try
            {
                var json = await PostMultiPartRequestForJsonAsync(new Uri($"{EndpointAddress}{NodesSuffix}"), new (string, string)[]
                {
                    ("OTCSTicket", AuthenticationToken)
                }, content);

                if (json.ContainsKey("results"))
                {
                    var raw = (json["results"]["data"]["properties"]["id"]).ToString();
                    return long.Parse(raw);
                }
                else
                {
                    throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                        $"An invalid response was returned by OpenText outcall - results element wasn't found");
                }
            }
            catch 
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"An invalid response was returned by OpenText outcall - results element wasn't found");
            }
        }

        /// <summary>
        /// Creates a folder underneath a given parent folder
        /// </summary>
        /// <param name="parentId">The id of the parent folder</param>
        /// <param name="name">The name for the folder</param>
        /// <param name="description">An optional description for the folder</param>
        /// <returns></returns>
        public async Task<long> CreateFolder(long parentId, string name, string description = "Automatically generated by Argon")
        {
            LogDebug(_log,$"{this.GetType()}: Attempting folder creation");
            ValidateConfiguration();
            var content = CreateMultiPartFormTemplate(
                new (string, string)[]
                {
                    ("type", 0.ToString()),
                    ("parent_id", parentId.ToString()),
                    ("name", name),
                    ("description", description)
                });
            try
            {
                var json = await PostMultiPartRequestForJsonAsync(new Uri($"{EndpointAddress}{NodesSuffix}"),
                    new (string, string)[]
                    {
                        ("OTCSTicket", AuthenticationToken)
                    },
                    content);
                if (json.ContainsKey("results"))
                {
                    var raw = (json["results"]["data"]["properties"]["id"]).ToString();
                    return long.Parse(raw);
                }
                else
                {
                    throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                        $"An invalid response was returned by OpenText outcall - results element wasn't found");
                }
            }
            catch (Exception ex)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"An exception was caught during an OpenText outcall: {ex.GetBaseException().Message}", ex);
            }

        }

        public async Task<bool> HasChildFolder(long parentId, string name)
        {
            return await GetChildFolderId(parentId, name) != 0;
        }

        /// <summary>
        /// Tries to retrieve the child folder id for a given folder
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<long> GetChildFolderId(long parentId, string name)
        {
            LogDebug(_log,$"{this.GetType()}: Attempting folder location");
            ValidateConfiguration();
            try
            {
                var uri = new Uri($"{EndpointAddress}/{NodesSuffix}/{parentId}/nodes");
                var json = await GetRequestForJsonAsync(uri, new (string, string)[]
                {
                    ("OTCSTicket", AuthenticationToken)
                }, new (string, string)[]
                {
                    ("where_name", name)
                });
                if (json.ContainsKey("results") && json["results"].HasValues)
                {
                    var node = json["results"][0];
                    return long.Parse(node["data"]["properties"]["id"].ToString());
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"An exception was caught during an OpenText outcall: {ex.GetBaseException().Message}", ex);
            }
        }

        /// <summary>
        /// The format for cached path entries
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GeneratePathCacheId(string path)
        {
            return $"path:{path}";
        }

        /// <summary>
        /// Adds a path and it's node id to the cache
        /// </summary>
        /// <param name="path">The path to be cached</param>
        /// <param name="id">The corresponding node id</param>
        /// <returns></returns>
        public async Task<CacheEntry> AddCachedPathId(string path, long id)
        {
            this.AssertNotNull(Cache, "No cache has been supplied!");
            return await Cache?.AddOrReplaceLongValueAsync(CachePartition, GeneratePathCacheId(path), id)!;
        }

        /// <summary>
        /// Returns the cache value associated with a given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<long> GetCachedPathId(string path)
        {
            if (Cache == null) return 0;
            var key = GeneratePathCacheId(path);
            if (await Cache.HasEntry(CachePartition, key))
            {
                var entry = await Cache.LookupEntry(CachePartition, key);
                if (entry != null)
                {
                    return entry.LongValue;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Attempts to create a path relative to the Enterprise node.  This method is cache-aware
        /// based on a non-null <see cref="IDbCache"/> instance being provided and a partition.
        /// </summary>
        /// <param name="path">A path of the format "/folder1/folder2../folderN</param>
        /// <param name="cache"></param>
        /// <returns>The node id</returns>
        /// <exception cref="OpenTextRestClient">Thrown if unable to create the path</exception>
        public async Task<long?> CreatePath(string path, bool cache = true)
        {
            long? id = 0;
            var cacheId = await GetCachedPathId(path);
            if (cacheId == 0)
            {
                long parentId = EnterpriseNodeId;
                var elements = path.Split('/');
                foreach (var element in elements)
                {
                    var childId = await GetChildFolderId(parentId, element);
                    if (childId == 0)
                    {
                        parentId = await CreateFolder(parentId, element);
                    }
                    else
                    {
                        parentId = childId;
                    }
                }

                if (cache)
                {
                    await Cache.AddOrReplaceLongValueAsync(CachePartition, GeneratePathCacheId(path), parentId);
                }

                return parentId;
            }
            else
            {
                return cacheId;
            }
        }
    }
}
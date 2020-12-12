using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Services.Core;
using Microsoft.AspNetCore.Http;
using JCS.Argon.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JCS.Argon.Services.VSP.Providers
{
    public class OpenTextRestClient : BaseRestClient
    {
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
        public static int EnterpriseNodeId = 2000;
        
        /// <summary>
        /// The REST api authentication endpoint suffix
        /// </summary>
        public static string AuthEndpointSuffix = "v1/auth";

        /// <summary>
        /// The v2 nodes api suffix
        /// </summary>
        public static string NodesSuffix = "v2/nodes";

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
        public string CachePartition {get; set;} = null!;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="cache"></param>
        public OpenTextRestClient(ILogger log, IDbCache? cache) : base(log)
        {
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
            string endpointAddress, string userName, string password) : base(log)
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
                _log.LogWarning($"{this.GetType()}: Failed to validate current configuration");
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
            _log.LogDebug($"{this.GetType()}: Attempting authentication");
            ValidateConfiguration();
            var content = CreateMultiPartFormTemplate(new (string, string)[]{
                ("username", UserName!),
                ("password", Password!)
            });
            
            try
            {
                var json = await PostMultiPartRequestForJsonAsync(new Uri($"{EndpointAddress}{AuthEndpointSuffix}"), content); 
                if (json.ContainsKey("ticket"))
                {
                    AuthenticationToken = (string)json["ticket"]!;
                    _log.LogDebug($"{this.GetType()}: Authentication successful");
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
    }
}
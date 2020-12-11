using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using Microsoft.AspNetCore.Http;
using JCS.Argon.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JCS.Argon.Services.VSP.Providers
{
    public class OpenTextRestClient : BaseRestClient
    {
        public static int EnterpriseNodeId = 2000;
        
        public static string MultiPartFormContentType = "multipart/form-data";
        
        public static string AuthEndpointSuffix = "v1/auth";

        public static string NodesSuffix = "v2/nodes/";

        public static string NodeChildrenSuffix = "nodes";

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
        
        public string? EndpointAddress { get; set; } = null!;

        public string? UserName { get; set; } = null!;

        public string? Password { get; set; } = null!;

        public string? AuthenticationToken { get; set; } = null!;


        public OpenTextRestClient(ILogger log) : base(log)
        {
        }

        public OpenTextRestClient(ILogger log, string endpointAddress, string userName, string password) : base(log)
        {
            if (endpointAddress.EndsWith('/'))
            {
                EndpointAddress = endpointAddress;
            }
            else
            {
                EndpointAddress = $"{endpointAddress}/";
            }

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
            var content = CreateMultiPartFormTemplate();
            content.Add(CreateStringFormField("username", UserName!));
            content.Add(CreateStringFormField("password", Password!));
            try
            {
                var json = await PostMultiPartRequestForJsonAsync(new Uri($"{EndpointAddress}{AuthEndpointSuffix}"), content); 
                if (json != null)
                {
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
                else
                {
                    throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                        $"OpenText authentication operation failed");
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
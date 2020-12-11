using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using JCS.Argon.Model.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using ContentDispositionHeaderValue = System.Net.Http.Headers.ContentDispositionHeaderValue;

namespace JCS.Argon.Services.VSP.Providers
{
    public class OpenTextRestClient
    {
        public static string MultiPartFormContentType = "multipart/form-data";
        
        public static string AuthEndpointSuffix = "v1/auth";

        /// <summary>
        /// Placeholder class for neater JSON deserialisation
        /// </summary>
        public class OpenTextAuthenticationResponse
        {
            public string? Ticket { get; set; }
        }

        /// <summary>
        /// Thrown if operations within the client fail
        /// </summary>
        public class OpenTextRestClientException : ResponseAwareException
        {
            public OpenTextRestClientException(int? statusHint, string? message) : base(statusHint, message)
            {
            }

            public OpenTextRestClientException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
            }
        }
        
        public string EndpointAddress { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string AuthenticationToken { get; set; } = null!;

        public HttpClient HttpClient { get; set; } = null!;

        public OpenTextRestClient()
        {
            
        }

        public OpenTextRestClient(string endpointAddress, string userName, string password)
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

        protected StringContent CreateStringFormField(string name, string value)
        {
            return new(value)
            {
                Headers =
                {
                    ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = name
                    }
                }
            };
        }

        protected void ValidateConfiguration()
        {
            if (EndpointAddress == null || UserName == null || Password == null)
            {
                throw new OpenTextRestClientException(StatusCodes.Status500InternalServerError,
                    $"OpenText REST Client is not currently configured correctly");
            }
        }
        
        /// <summary>
        /// Attempts an authentication operation and stashes away the authentication ticket/token
        /// </summary>
        /// <exception cref="OpenTextRestClientException"></exception>
        public async void Authenticate()
        {
            ValidateConfiguration();
            var payload = new MultipartFormDataContent();
            payload.Add(CreateStringFormField("username", UserName));
            payload.Add(CreateStringFormField("password", Password));
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{EndpointAddress}{AuthEndpointSuffix}"),
                Headers =
                {
                    {"Content-Type", MultiPartFormContentType}
                },
                Content = payload
            };
            try
            {
                var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var authResponse = await (response.Content.ReadFromJsonAsync<OpenTextAuthenticationResponse>());
                if (authResponse != null && authResponse.Ticket != null)
                {
                    AuthenticationToken = authResponse.Ticket;
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
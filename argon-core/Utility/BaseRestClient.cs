using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Services.VSP.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JCS.Argon.Utility
{
    public abstract class BaseRestClient
    {

        public sealed class BaseRestClientException : ResponseAwareException
        {
            public BaseRestClientException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(BaseRestClient);
            }

            public BaseRestClientException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(BaseRestClient);
            }
        }
        
        public HttpClient HttpClient { get; set; } = null!;
        
        protected ILogger _log;

        public BaseRestClient(ILogger log)
        {
            _log = log;
        }

        /// <summary>
        /// Creates a multi-part form body, with a guid-based boundary
        /// </summary>
        /// <returns></returns>
        protected MultipartFormDataContent CreateMultiPartFormTemplate()
        {
            var boundary = Guid.NewGuid().ToString();
            var template = new MultipartFormDataContent(boundary);
            template.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/form-data; boundary={boundary}");
            return template;
        }

        /// <summary>
        /// Creates a multi-part form body, by converting a passed in array of string tuples into a series of
        /// string field body parts
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        protected MultipartFormDataContent CreateMultiPartFormTemplate((string, string)[] fields)
        {
            var boundary = Guid.NewGuid().ToString();
            var template = new MultipartFormDataContent(boundary);
            template.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/form-data; boundary={boundary}");
            foreach(var field in fields){
                template.Add(CreateStringFormField(field.Item1, field.Item2));
            }
            return template;
        }

        /// <summary>
        /// Convenience wrapper around async multi-part POST requests
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="checkResponseCodes"></param>
        /// <returns></returns>
        protected async Task<JObject> PostMultiPartRequestForJsonAsync(Uri uri, (string, string)[] headers, MultipartFormDataContent content, bool checkResponseCodes = true)
        {
            this.AssertNotNull(HttpClient, "HttpClient is null...unexpected");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMessage.Content = content;
            foreach (var header in headers)
            {
                requestMessage.Headers.Add(header.Item1, header.Item2);
            }
            var response = await HttpClient.SendAsync(requestMessage);
            if (checkResponseCodes) response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                return JObject.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                _log.LogWarning($"{this.GetType()}: Invalid JSON returned in response to a request");
                throw new BaseRestClientException(StatusCodes.Status500InternalServerError,
                    $"Invalid JSON response received", ex);
            }
        }

        protected Uri AppendQueryStringParametersToUri(Uri source, (string, string)[] fields)
        {
            var sb = new StringBuilder();
            foreach(var field in fields)
            {
                sb.Append($"{field.Item1}={field.Item2}&");
            }
            var queryString = Uri.EscapeDataString(sb.ToString().TrimEnd('&'));
            return new Uri($"{source.ToString()}?{queryString}");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="queryParams"></param>
        /// <param name="headers"></param>
        /// <param name="checkResponseCodes"></param>
        /// <returns></returns>
        /// <exception cref="BaseRestClientException"></exception>
        protected async Task<JObject> GetRequestForJsonAsync(Uri uri, (string, string)[] headers, 
            (string, string)[] queryParams, bool checkResponseCodes = true)
        {
            this.AssertNotNull(HttpClient, "HttpClient is null...unexpected!");
            uri = AppendQueryStringParametersToUri(uri, queryParams);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            foreach (var header in headers)
            {
                requestMessage.Headers.Add(header.Item1, header.Item2);
            }
            var response = await HttpClient.SendAsync(requestMessage);
            if (checkResponseCodes) response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                return JObject.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                _log.LogWarning($"{this.GetType()}: Invalid JSON returned in response to a request");
                throw new BaseRestClientException(StatusCodes.Status500InternalServerError,
                    $"Invalid JSON response received", ex);
            }
        }

        /// <summary>
        /// Convenience method for creating string form fields
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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
    }
}
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Services.VSP.Providers;
using JCS.Neon.Glow.Types;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Utility
{
    public abstract class BaseRestClient
    {
        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext<BaseRestClient>();


        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseRestClient()
        {
        }

        /// <summary>
        /// The wrapped <see cref="HttpClient"/>
        /// </summary>
        public HttpClient HttpClient { get; set; } = null!;

        /// <summary>
        /// Creates a multi-part form body, with a guid-based boundary
        /// </summary>
        /// <returns></returns>
        protected MultipartFormDataContent CreateMultiPartFormTemplate()
        {
            LogMethodCall(_log);
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
            LogMethodCall(_log);
            var boundary = Guid.NewGuid().ToString();
            var template = new MultipartFormDataContent(boundary);
            template.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/form-data; boundary={boundary}");
            foreach (var field in fields)
            {
                template.Add(CreateStringFormField(field.Item1, field.Item2));
            }

            return template;
        }

        /// <summary>
        /// Convenience wrapper around async multi-part POST requests
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="content"></param>
        /// <param name="checkResponseCodes"></param>
        /// <returns></returns>
        protected async Task<JObject> PostMultiPartRequestForJsonAsync(Uri uri, (string, string)[] headers,
            MultipartFormDataContent content, bool checkResponseCodes = true)
        {
            LogMethodCall(_log);
            this.AssertNotNull(HttpClient, "HttpClient is null...unexpected");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMessage.Content = content;
            MergeHeaders(requestMessage, headers);
            var response = await HttpClient.SendAsync(requestMessage);
            if (checkResponseCodes) response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                return JObject.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                LogWarning(_log, $"{this.GetType()}: Invalid JSON returned in response to a request");
                throw new BaseRestClientException(StatusCodes.Status500InternalServerError,
                    $"Invalid JSON response received", ex);
            }
        }

        /// <summary>
        /// Appends a series of query string parameters to a <see cref="Uri"/>
        /// </summary>
        /// <param name="source">The original Uri</param>
        /// <param name="fields">A series of string pairs</param>
        /// <returns></returns>
        protected Uri AppendQueryStringParametersToUri(Uri source, (string, string)[] fields)
        {
            LogMethodCall(_log);
            if (fields != null)
            {
                var sb = new StringBuilder();
                foreach (var field in fields)
                {
                    sb.Append($"{field.Item1}={field.Item2}&");
                }

                var queryString = Uri.EscapeDataString(sb.ToString().TrimEnd('&'));
                return new Uri($"{source.ToString()}?{queryString}");
            }
            else
            {
                return source;
            }
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
            LogMethodCall(_log);
            var response = await GetRequest(uri, headers, queryParams, checkResponseCodes);
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                return JObject.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                LogWarning(_log, $"{this.GetType()}: Invalid JSON returned in response to a request");
                throw new BaseRestClientException(StatusCodes.Status500InternalServerError,
                    $"Invalid JSON response received", ex);
            }
        }

        /// <summary>
        /// Generic method for issuing a GET request with headers and query parameters
        /// </summary>
        /// <param name="uri">The Uri to issue the GEt request against</param>
        /// <param name="headers">A series of string pairs which are converted to headers</param>
        /// <param name="queryParams">A series of query string parameters which are appended to the source uri</param>
        /// <param name="checkResponseCodes">If true, then response codes are checked and optional exceptions are thrown</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> GetRequest(Uri uri, (string, string)[] headers,
            (string, string)[] queryParams, bool checkResponseCodes = true)
        {
            LogMethodCall(_log);
            HttpResponseMessage response;
            LogMethodCall(_log);
            this.AssertNotNull(HttpClient, "HttpClient is null...unexpected!");
            uri = AppendQueryStringParametersToUri(uri, queryParams);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            MergeHeaders(requestMessage, headers);
            response = await HttpClient.SendAsync(requestMessage);
            if (checkResponseCodes) response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="queryParams"></param>
        /// <param name="checkResponseCodes"></param>
        /// <returns></returns>
        protected async Task<Pair<string, Stream>> GetRequestForContentStream(Uri uri, (string, string)[] headers,
            (string, string)[] queryParams, bool checkResponseCodes = true)
        {
            LogMethodCall(_log);
            var response = await GetRequest(uri, headers, queryParams, checkResponseCodes);
            var stream = await response.Content.ReadAsStreamAsync();
            return new Pair<string, Stream>(response.Content.Headers.ContentType.ToString(), stream);
        }

        /// <summary>
        /// Merges a list of header pairs into the set of headers for a given request
        /// </summary>
        /// <param name="message">The <see cref="HttpRequestMessage"/></param>
        /// <param name="headers">An array of string pairs</param>
        private void MergeHeaders(HttpRequestMessage message, (string, string)[] headers)
        {
            foreach (var header in headers)
            {
                message.Headers.Add(header.Item1, header.Item2);
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
            LogMethodCall(_log);
            return new StringContent(value)
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
    }
}
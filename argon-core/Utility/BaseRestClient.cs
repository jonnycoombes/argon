#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Neon.Glow.Types;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Utility
{
    public abstract class BaseRestClient
    {
        /// <summary>
        ///     Just a constant for the standard set cookie header
        /// </summary>
        protected const string SetCookieHeader = "Set-Cookie";

        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<BaseRestClient>();


        /// <summary>
        ///     The wrapped <see cref="HttpClient" />
        /// </summary>
        public HttpClient HttpClient { get; set; } = null!;

        /// <summary>
        ///     Creates a multi-part form body, with a guid-based boundary
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
        ///     Creates a multi-part form body, by converting a passed in array of string tuples into a series of
        ///     string field body parts
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        protected static MultipartFormDataContent CreateMultiPartFormTemplate(IEnumerable<(string, string)> fields)
        {
            LogMethodCall(_log);
            var boundary = Guid.NewGuid().ToString();
            var template = new MultipartFormDataContent(boundary);
            template.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/form-data; boundary={boundary}");
            foreach (var (item1, item2) in fields)
            {
                template.Add(CreateStringFormField(item1, item2));
            }

            return template;
        }

        /// <summary>
        ///     Convenience wrapper around async multi-part POST requests
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="content"></param>
        /// <param name="checkResponseCodes"></param>
        /// <returns></returns>
        protected async Task<JObject> PostMultiPartRequestForJsonAsync(Uri uri, IEnumerable<(string, string)> headers,
            MultipartFormDataContent content, bool checkResponseCodes = true)
        {
            LogMethodCall(_log);
            this.AssertNotNull(HttpClient, "HttpClient is null...unexpected");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri) {Content = content};
            MergeHeaders(requestMessage, headers);
            var response = await HttpClient.SendAsync(requestMessage);
            if (checkResponseCodes)
            {
                response.EnsureSuccessStatusCode();
            }

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                return JObject.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                LogWarning(_log, $"{GetType()}: Invalid JSON returned in response to a request");
                throw new BaseRestClientException(StatusCodes.Status500InternalServerError,
                    "Invalid JSON response received", ex);
            }
        }

        /// <summary>
        ///     Appends a series of query string parameters to a <see cref="Uri" />
        /// </summary>
        /// <param name="source">The original Uri</param>
        /// <param name="fields">A series of string pairs</param>
        /// <returns></returns>
        private static Uri AppendQueryStringParametersToUri(Uri source, IEnumerable<(string, string)>? fields)
        {
            LogMethodCall(_log);
            if (fields == null)
            {
                return source;
            }

            var sb = new StringBuilder();
            foreach (var (item1, item2) in fields)
            {
                sb.Append($"{item1}={item2}&");
            }

            var queryString = Uri.EscapeDataString(sb.ToString().TrimEnd('&'));
            return new Uri($"{source}?{queryString}");
        }

        /// <summary>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="queryParams"></param>
        /// <param name="headers"></param>
        /// <param name="checkResponseCodes"></param>
        /// <returns></returns>
        /// <exception cref="BaseRestClientException"></exception>
        protected async Task<JObject> GetRequestForJsonAsync(Uri uri, IEnumerable<(string, string)> headers,
            IEnumerable<(string, string)> queryParams, bool checkResponseCodes = true)
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
                LogWarning(_log, $"{GetType()}: Invalid JSON returned in response to a request");
                throw new BaseRestClientException(StatusCodes.Status500InternalServerError,
                    "Invalid JSON response received", ex);
            }
        }

        /// <summary>
        ///     Generic method for issuing a GET request with headers and query parameters
        /// </summary>
        /// <param name="uri">The Uri to issue the GEt request against</param>
        /// <param name="headers">A series of string pairs which are converted to headers</param>
        /// <param name="queryParams">A series of query string parameters which are appended to the source uri</param>
        /// <param name="checkResponseCodes">If true, then response codes are checked and optional exceptions are thrown</param>
        /// <returns>The returned <see cref="HttpResponseMessage" /></returns>
        protected async Task<HttpResponseMessage> GetRequest(Uri uri, IEnumerable<(string, string)>? headers,
            IEnumerable<(string, string)>? queryParams, bool checkResponseCodes = true)
        {
            LogMethodCall(_log);
            this.AssertNotNull(HttpClient, "HttpClient is null...unexpected!");
            uri = AppendQueryStringParametersToUri(uri, queryParams);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            MergeHeaders(requestMessage, headers);
            var response = await HttpClient.SendAsync(requestMessage);
            if (checkResponseCodes)
            {
                response.EnsureSuccessStatusCode();
            }

            return response;
        }

        /// <summary>
        ///     Generic method for issuing a DELETE request with headers and query parameters
        /// </summary>
        /// <param name="uri">The Uri to issue the DELETE request against</param>
        /// <param name="headers">A series of string pairs which are converted to headers within the request</param>
        /// <param name="queryParams">A series of query string parameters which are appended to the source uri</param>
        /// <param name="checkResponseCodes">If ture, then response codes are checked and optional exception are thrown</param>
        /// <returns>The returned <see cref="HttpResponseMessage" /></returns>
        protected async Task<HttpResponseMessage> DeleteRequest(Uri uri, IEnumerable<(string, string)>? headers,
            IEnumerable<(string, string)>? queryParams, bool checkResponseCodes = true)
        {
            LogMethodCall(_log);
            this.AssertNotNull(HttpClient, "HttpClient is null...unexpected!");
            uri = AppendQueryStringParametersToUri(uri, queryParams);
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            MergeHeaders(requestMessage, headers);
            var response = await HttpClient.SendAsync(requestMessage);
            if (checkResponseCodes)
            {
                response.EnsureSuccessStatusCode();
            }

            return response;
        }

        /// <summary>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="queryParams"></param>
        /// <param name="checkResponseCodes"></param>
        /// <returns></returns>
        protected async Task<Pair<string, Stream>> GetRequestForContentStream(Uri uri, IEnumerable<(string, string)> headers,
            IEnumerable<(string, string)> queryParams, bool checkResponseCodes = true)
        {
            LogMethodCall(_log);
            var response = await GetRequest(uri, headers, queryParams, checkResponseCodes);
            var stream = await response.Content.ReadAsStreamAsync();
            return new Pair<string, Stream>(response.Content.Headers.ContentType.ToString(), stream);
        }

        /// <summary>
        ///     Merges a list of header pairs into the set of headers for a given request
        /// </summary>
        /// <param name="message">The <see cref="HttpRequestMessage" /></param>
        /// <param name="headers">An array of string pairs</param>
        private static void MergeHeaders(HttpRequestMessage message, IEnumerable<(string, string)>? headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var (item1, item2) in headers)
            {
                message.Headers.Add(item1, item2);
            }
        }

        /// <summary>
        ///     Convenience method for creating string form fields
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static StringContent CreateStringFormField(string name, string value)
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

        private sealed class BaseRestClientException : ResponseAwareException
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
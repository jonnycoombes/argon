using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JCS.Argon.Services.VSP.Providers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JCS.Argon.Utility
{
    public abstract class BaseRestClient
    {
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
            template.Headers.ContentType = MediaTypeHeaderValue.Parse($"{OpenTextRestClient.MultiPartFormContentType}; boundary={boundary}");
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
            template.Headers.ContentType = MediaTypeHeaderValue
                .Parse($"{OpenTextRestClient.MultiPartFormContentType}; boundary={boundary}");
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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected async Task<JObject?> PostMultiPartRequestForJsonAsync(Uri uri, MultipartFormDataContent content, bool checkResponseCodes = true)
        {
            this.AssertNotNull(HttpClient, "HttpClient is null...unexpected");
            var response = await HttpClient.PostAsync(uri, content);
            if (checkResponseCodes) response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                return JObject.Parse(json);
            }
            catch (JsonReaderException)
            {
                _log.LogWarning($"{this.GetType()}: Invalid JSON returned in response to a request");
                return null;
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
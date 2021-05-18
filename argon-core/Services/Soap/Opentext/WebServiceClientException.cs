using System;

namespace JCS.Argon.Services.Soap.Opentext
{
    /// <summary>
    ///     Exception specific to the <see cref="WebServiceClient" /> class
    /// </summary>
    public class WebServiceClientException : Exception
    {
        public WebServiceClientException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public WebServiceClientException(string? message) : base(message)
        {
        }
    }
}
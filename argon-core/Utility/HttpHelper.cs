#region

using System.Net;
using Microsoft.AspNetCore.Http;

#endregion

namespace JCS.Argon.Utility
{
    /// <summary>
    ///     Simple Http related helpers and utilities
    /// </summary>
    public static class HttpHelper
    {
        public static string BuildEndpointFromContext(HttpContext context)
        {
            var host = context.Request.Host;
            if (context.Request.Scheme == "https")
            {
                if (host.Port == 443)
                    return $"{context.Request.Scheme}://{Dns.GetHostName()}{context.Request.Path}";
                return $"{context.Request.Scheme}://{Dns.GetHostName()}:{context.Request.Host.Port}{context.Request.Path}";
            }

            if (host.Port == 80)
                return $"{context.Request.Scheme}://{Dns.GetHostName()}{context.Request.Path}";
            return $"{context.Request.Scheme}://{Dns.GetHostName()}:{context.Request.Host.Port}{context.Request.Path}";
        }
    }
}
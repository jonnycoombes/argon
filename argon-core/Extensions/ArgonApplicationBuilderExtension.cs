#region

using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using static System.DateTime;

#endregion

namespace JCS.Argon.Extensions
{
    /// <summary>
    ///     Class which can be used in order to add middleware etc...
    /// </summary>
    public static class ArgonApplicationBuilderExtension
    {
        /// <summary>
        ///     Name for the Argon timings header - prefixed with X following convention for extended headers
        /// </summary>
        public static string X_ARGON_TIMINGS_TOTAL = "X-Argon-Timing";

        /// <summary>
        ///     Name for the Argon timestamp header - prefixed with X following convention for extended headers
        /// </summary>
        public static string X_ARGON_TIMESTAMP = "X-Argon-Timestamp";

        /// <summary>
        ///     Extension which just injects timings code before and after a given request, which can then be injected into the
        ///     response headers
        /// </summary>
        /// <param name="app">The current <see cref="IApplicationBuilder" /></param>
        /// <returns></returns>
        public static IApplicationBuilder UseArgonTelemetry(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var timer = new Stopwatch();
                timer.Start();
                #pragma warning disable 1998
                context.Response.OnStarting(async () =>
                    #pragma warning restore 1998
                {
                    timer.Stop();
                    context.Response.Headers.Add(X_ARGON_TIMINGS_TOTAL, timer.ElapsedMilliseconds.ToString());
                    context.Response.Headers.Add(X_ARGON_TIMESTAMP, UtcNow.ToString("HH:m:s tt zzz"));
                });
                await next.Invoke();
            });
            return app;
        }
    }
}
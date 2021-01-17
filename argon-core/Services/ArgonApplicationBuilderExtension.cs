using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using static System.DateTime;

namespace JCS.Argon.Extensions
{
    /// <summary>
    /// Class which can be used in order to add middleware etc...
    /// </summary>
    public static class ArgonApplicationBuilderExtension
    {
        public static string X_ARGON_TIMINGS_TOTAL = "X-Argon-Timing";

        public static string X_ARGON_TIMESTAMP = "X-Argon-Timestamp";

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
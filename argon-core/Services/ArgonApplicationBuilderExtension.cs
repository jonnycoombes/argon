﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Class which can be used in order to add middleware etc...
    /// </summary>
    public static class ArgonApplicationBuilderExtension
    {
        public static string X_ARGON_TIMINGS_TOTAL = "X-Argon-Timing-Total-Ms";
        
        public static IApplicationBuilder UseArgonTimings(this IApplicationBuilder app)
        {
            app.Use(async (context,next) =>
            {
                var timer = new Stopwatch();
                timer.Start();
#pragma warning disable 1998
                context.Response.OnStarting(async () =>
#pragma warning restore 1998
                {
                    timer.Stop();
                    context.Response.Headers.Add(X_ARGON_TIMINGS_TOTAL, timer.ElapsedMilliseconds.ToString());
                });
                await next.Invoke();
            });
            return app;
        }
    }
}
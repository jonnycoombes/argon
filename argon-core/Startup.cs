#region

using System.Text.Json;
using JCS.Argon.Extensions;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon
{
    public class Startup
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<Startup>();

        /// <summary>
        ///     Constructor - just inject a config and current environment
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            LogMethodCall(_log);
            Configuration = configuration;
            Environment = env;
        }

        /// <summary>
        ///     Current <see cref="IWebHostEnvironment" />
        /// </summary>
        private IWebHostEnvironment Environment { get; }

        /// <summary>
        ///     Current total <see cref="IConfiguration" />
        /// </summary>
        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            LogMethodCall(_log);
            services.RegisterDbContext(Configuration, Environment);
            services.RegisterArgonConfig(Configuration);
            services.RegisterArgonServices(Configuration);
            LogInformation(_log, "Service configuration and registration completed");
        }

        /// <summary>
        ///     This is a NO-OP at the moment
        /// </summary>
        /// <param name="app">The current <see cref="IApplicationBuilder" /></param>
        protected void EnsureDatabaseIsCreated(IApplicationBuilder app)
        {
            LogMethodCall(_log);
            Log.Information("Checking and ensuring the that target database exists");
        }

        /// <summary>
        ///     Configure a centralised last-resort exception handler
        /// </summary>
        /// <param name="app">The current <see cref="IApplicationBuilder" /></param>
        protected static void ConfigureGlobalExceptionHandling(IApplicationBuilder app)
        {
            LogMethodCall(_log);
            LogInformation(_log, "Registering global exception handling logic");
            app.UseExceptionHandler(errorApp =>
            {
                var handler = errorApp.ApplicationServices.GetService<IResponseExceptionHandler>();
                errorApp.Run(async context =>
                {
                    var payload = handler?.GenerateExceptionResponseFromContext(context);
                    if (payload != null)
                    {
                        context.Response.Headers.Clear();
                        context.Response.StatusCode = payload.HttpResponseCode!;
                        context.Response.ContentType = "application/json; utf-8";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                        await context.Response.CompleteAsync();
                    }
                    else
                    {
                        //context.Response.Headers.Clear();
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsync("Failed to determine underlying cause - please contact a system administrator");
                        await context.Response.CompleteAsync();
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            LogMethodCall(_log);
            LogInformation(_log,
                env.IsDevelopment() ? "Starting within a development environment" : "Starting within a non-development environment");

            ConfigureGlobalExceptionHandling(app);
            app.UseSwagger();
            var apiOptions = new ApiOptions();
            Configuration.GetSection(ApiOptions.ConfigurationSection).Bind(apiOptions);
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Argon v1");
                c.RoutePrefix = "swagger";
            });

            app.UseSerilogRequestLogging();
            app.UseResponseCompression();
            app.UseArgonTelemetry();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
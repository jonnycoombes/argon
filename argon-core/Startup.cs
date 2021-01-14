using System;
using JCS.Argon.Extensions;
using JCS.Argon.Contexts;
using JCS.Argon.Services.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace JCS.Argon
{
    public class Startup
    {

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        /// <summary>
        /// Current <see cref="IWebHostEnvironment"/>
        /// </summary>
        private IWebHostEnvironment Environment {get;}

        /// <summary>
        /// Current total <see cref="IConfiguration"/>
        /// </summary>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon.Startup")
                .Information("Configuring services");
            services.RegisterDbContext(Configuration, Environment);
            services.RegisterArgonConfig(Configuration);
            services.RegisterArgonServices(Configuration);
            Log.Information("Service configuration and registration completed");
        }

        protected void EnsureDatabaseIsCreated(IApplicationBuilder app)
        {
            Log.Information("Checking and ensuring the that target database exists");
        }

        protected void ConfigureGlobalExceptionHandling(IApplicationBuilder app, ILogger<Startup> log)
        {
            log.LogInformation("Registering global exception handling logic");
            app.UseExceptionHandler(errorApp =>
            {
                var handler = errorApp.ApplicationServices.GetService<IResponseExceptionHandler>();
                errorApp.Run(async context =>
                {
                    var payload= handler?.GenerateExceptionResponseFromContext(context);
                    if (payload != null)
                    {
                        context.Response.Headers.Clear();
                        context.Response.StatusCode = ((int) payload.HttpResponseCode)!;
                        context.Response.ContentType = "application/json; utf-8";
                        await context.Response.WriteAsJsonAsync(payload);
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> log)
        {
            if (env.IsDevelopment() || Environment.IsEnvironment("WinDevelopment"))
            {
                log.LogInformation("Starting within a development environment");
                if (Environment.IsEnvironment("WinDevelopment")) log.LogDebug("Currently running on a Windows development platform");
                //app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Argon v1"));
                log.LogInformation("Enabling request logging...for development purposes");
                app.UseSerilogRequestLogging();
            }
            else
            {
                log.LogInformation("Starting within a non-development environment");
            }
            
            ConfigureGlobalExceptionHandling(app, log);
            app.UseResponseCompression();
            app.UseArgonTelemetry();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

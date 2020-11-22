using System;
using System.IO;
using System.Reflection;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Core;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;

namespace JCS.Argon
{
    public class Startup
    {
        private IWebHostEnvironment Environment {get;}

        public IConfiguration Configuration { get; } 
        
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        /// <summary>
        /// Register the db context, optional branching here to allow for different connection strings based on the
        /// currently configured environment
        /// </summary>
        /// <param name="services"></param>
        /// TODO - externalise the retry and delay parameters to the app configuration
        private void RegisterDbContext(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon.Startup")
                .Information("Registering Db context");
            try
            {
                if (Environment.IsDevelopment())
                {
                    Log.ForContext("SourceContext", "JCS.Argon.Startup")
                        .Information("In development so using default connection string");
                    services.AddDbContext<SqlDbContext>(options =>
                    {
                        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), 
                            sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 10,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                        });
                        options.EnableDetailedErrors();
                        
                    });
                }
                else
                {
                    services.AddDbContext<SqlDbContext>(options =>
                        options
                            .UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
                }
            }
            catch (Exception ex)
            {
                Log.ForContext("SourceContext", "JCS.Argon.Startup")
                    .Fatal("Caught an exception whilst attempting to register Db context", ex);
            }
        }

        

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon.Startup")
                .Information("Configuring services");
            RegisterDbContext(services);
            services.RegisterArgonConfig(Configuration);
            services.RegisterArgonServices();
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
                        context.Response.StatusCode = ((int) payload.HttpResponseCode)!;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(payload);
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsync("Failed to determine underlying cause - please contact a system administrator");
                    }
                });
            });
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> log)
        {
            if (env.IsDevelopment())
            {
                log.LogInformation("Starting within a development environment");
                app.UseDeveloperExceptionPage();
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

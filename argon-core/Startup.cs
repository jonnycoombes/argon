using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Core;
using ILogger = Serilog.ILogger;

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
                        options
                            .UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
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

        private void RegisterCoreConfiguration(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon.Startup")
                .Information("Binding to main API configuration");
            try
            {
                var apiConfiguration = new ApiConfiguration
                {
                    VspConfiguration = new VSPConfiguration(Configuration)
                };
                services.AddSingleton(apiConfiguration);
            }
            catch (Exception ex)
            {
                Log.ForContext("SourceContext", "JCS.Argon.Startup")
                    .Fatal("Caught exception whilst attempting to bind to API configuration", ex);
            }
        }
        
        /// <summary>
        /// Do anything specific to controller bindings, Swagger configuratino etc...
        /// in here
        /// </summary>
        /// <param name="services">Current services collection</param>
        protected void ConfigureApiServices(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon.Startup")
                .Information("Configuring controllers and Swagger components");
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "Argon - Content Service Layer", 
                    Version = "v1",
                    Description = $"Glencore Content Service Layer. (Build Version: {new AppVersion().ToString()})" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            }); 
            
        }

        /// <summary>
        /// Register all application-specific services such as the VSP registry etc...
        /// </summary>
        /// <param name="services">The current services collection</param>
        protected void ConfigureCoreServices(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon.Startup")
                .Information("Configuring core API services");
            Log.ForContext("SourceContext", "JCS.Argon.Startup")
                .Information("Registering a scoped VSP factory");
            services.AddScoped<IVSPFactory, VSPFactory>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon.Startup")
                .Information("Configuring services");
            RegisterCoreConfiguration(services);
            RegisterDbContext(services);
            ConfigureApiServices(services);
            ConfigureCoreServices(services);
            Log.Information("Service configuration and registration completed");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                logger.LogInformation("Starting within a development environment");
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Argon v1"));
                logger.LogInformation("Enabling request logging...for development purposes");
                app.UseSerilogRequestLogging();
            }
            else
            {
                logger.LogInformation("Starting within a non-development environment");
            }

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

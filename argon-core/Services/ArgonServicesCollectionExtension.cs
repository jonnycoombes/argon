using System;
using System.IO;
using System.Reflection;
using JCS.Argon;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Core;
using JCS.Argon.Services.VSP;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ArgonServicesCollectionExtension
    {
        /// <summary>
        /// Adds any argon-specific configuration elements into the IoC container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterArgonConfig(this IServiceCollection services, IConfiguration config)
        {
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Injecting Argon configuration options");
            services.AddOptions();
            services.Configure<VSPConfiguration>(config.GetSection("vsp"));
            return services;
        }

        /// <summary>
        /// All Argon-specific services should be added to the IoC container here
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterArgonServices(this IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Registering Argon services");
            RegisterApiServices(services);
            RegisterCoreServices(services);
            return services;
        }
        
        /// <summary>
        /// Do anything specific to controller bindings, Swagger configuratino etc...
        /// in here
        /// </summary>
        /// <param name="services">Current services collection</param>
        private static void RegisterApiServices(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Configuring controllers and Swagger components");
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "Argon - Content Service Layer", 
                    Version = $"v1 ({new AppVersion().ToString()})",
                    Description = $"Argon. (Build Version: {new AppVersion().ToString()})" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            }); 
            
        }
        
        /// <summary>
        /// Register all application-specific services such as the VSP registry etc...
        /// Note the differences between whether services are scoped (basically per-session) or
        /// singleton
        /// </summary>
        /// <param name="services">The current services collection</param>
        private static void RegisterCoreServices(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Configuring core API services");
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Registering a scoped VSP factory");
            services.AddScoped<IVSPFactory, VSPFactory>();
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Registering a scoped collection manager");
            services.AddScoped<ICollectionManager, CollectionManager>();
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Registering global response exception handler");
            services.AddSingleton<IResponseExceptionHandler, ResponseExceptionHandler>();
        }
    }
}
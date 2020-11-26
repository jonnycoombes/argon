using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JCS.Argon;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Core;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
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

        private static void ConfigureResponseCompression(IServiceCollection services)
        {
            services.AddResponseCompression(options => {
                options.EnableForHttps= true;
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/json" });
            });
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
            ConfigureResponseCompression(services);
            return services;
        }
        
        /// <summary>
        /// Do anything specific to controller bindings, Swagger configuration etc...
        /// Specific alterations here to ignore null values in Json serialisation to
        /// minimise payload size, and also to de-clutter responses.
        /// in here
        /// </summary>
        /// <param name="services">Current services collection</param>
        private static void RegisterApiServices(IServiceCollection services)
        {
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Configuring controllers and Swagger components");
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "Argon - Content Service Layer", 
                    Version = $"v1 ({new AppVersion().ToString()})",
                    Description = $"Argon. (Build Version: {new AppVersion().ToString()})",
                    Contact = new OpenApiContact
                    {
                        Name = "Jonny Coombes",
                        Email = "jcoombes@jcs-software.co.uk"
                    }
                });
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
            services.AddSingleton<IVSPManager, VSPManager>();
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Registering a scoped property group manager");
            services.AddScoped<IPropertyGroupManager, PropertyGroupManager>();
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Registering a scoped collection manager");
            services.AddScoped<ICollectionManager, CollectionManager>();
            Log.ForContext("SourceContext", "JCS.Argon")
                .Information("Registering global response exception handler");
            services.AddSingleton<IResponseExceptionHandler, ResponseExceptionHandler>();
        }
    }
}
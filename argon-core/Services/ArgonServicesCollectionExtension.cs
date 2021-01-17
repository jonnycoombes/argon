using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Core;
using JCS.Argon.Services.VSP;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;
using static JCS.Neon.Glow.Helpers.General.ReflectionHelpers;

namespace JCS.Argon.Extensions
{
    public static class ArgonServicesCollectionExtension
    {
        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext(typeof(ArgonServicesCollectionExtension));

        /// <summary>
        /// Register the db context, optional branching here to allow for different connection strings based on the
        /// currently configured environment
        /// </summary>
        /// <param name="services"></param>
        /// <<param name="config"></param>
        /// <<param name="hostEnvironment"></param>
        public static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration config,
            IWebHostEnvironment hostEnvironment)
        {
            LogMethodCall(_log);
            try
            {
                if (hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("WinDevelopment"))
                {
                    LogInformation(_log, "In development so using default connection string");
                    services.AddDbContext<SqlDbContext>(options =>
                    {
                        options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                            sqlServerOptionsAction: sqlOptions => { });
                        options.EnableDetailedErrors();
                    });
                }
                else
                {
                    services.AddDbContext<SqlDbContext>(options =>
                        options
                            .UseSqlServer(config.GetConnectionString("DefaultConnection")));
                }
            }
            catch (Exception ex)
            {
                LogExceptionError(_log, ex);
                LogError(_log, "Caught an exception whilst attempting to register Db context");
            }

            return services;
        }

        /// <summary>
        /// Adds any argon-specific configuration elements into the IoC container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterArgonConfig(this IServiceCollection services, IConfiguration config)
        {
            LogMethodCall(_log);
            services.AddOptions();
            services.Configure<ApiOptions>(config.GetSection(ApiOptions.ConfigurationSection));
            return services;
        }

        private static void ConfigureResponseCompression(IServiceCollection services)
        {
            LogMethodCall(_log);
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] {"application/pdf"});
            });
            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });
            services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });
        }

        /// <summary>
        /// All Argon-specific services should be added to the IoC container here
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterArgonServices(this IServiceCollection services, IConfiguration config)
        {
            LogMethodCall(_log);
            RegisterApiServices(services, config);
            RegisterCoreServices(services, config);
            RegisterHttpClientServices(services, config);
            ConfigureResponseCompression(services);
            return services;
        }

        /// <summary>
        /// Register any typed/untyped IHttpClientFactories here
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        private static void RegisterHttpClientServices(IServiceCollection services, IConfiguration config)
        {
            LogMethodCall(_log);
            services.AddHttpClient<IVirtualStorageManager, VirtualStorageManager>();
        }

        /// <summary>
        /// Do anything specific to controller bindings, Swagger configuration etc...
        /// Specific alterations here to ignore null values in Json serialisation to
        /// minimise payload size, and also to de-clutter responses.
        /// in here
        /// </summary>
        /// <param name="services">Current services collection</param>
        /// <param name="config"></param>
        private static void RegisterApiServices(IServiceCollection services, IConfiguration config)
        {
            LogMethodCall(_log);
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Argon - Content Service Layer",
                    Version = $"v1 ({GetApplicationAssemblyVersion()})",
                    Description = $"Argon. (Build Version: {GetApplicationAssemblyVersion()})",
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
        /// <param name="config"></param>
        private static void RegisterCoreServices(IServiceCollection services, IConfiguration config)
        {
            LogMethodCall(_log);
            LogInformation(_log, "Registering a scoped VSP factory");
            services.AddSingleton<IVirtualStorageManager, VirtualStorageManager>();
            LogInformation(_log, "Registering a scoped property group manager");
            services.AddScoped<IPropertyGroupManager, PropertyGroupManager>();
            LogInformation(_log, "Registering a scoped constraint group manager");
            services.AddScoped<IConstraintGroupManager, ConstraintGroupManager>();
            LogInformation(_log, "Registering a scoped item manager");
            services.AddScoped<IItemManager, ItemManager>();
            LogInformation(_log, "Registering a scoped collection manager");
            services.AddScoped<ICollectionManager, CollectionManager>();
            LogInformation(_log, "Registering db cache");
            services.AddScoped<IDbCache, DbCache>();
            LogInformation(_log, "Registering global response exception handler");
            services.AddSingleton<IResponseExceptionHandler, ResponseExceptionHandler>();
        }
    }
}
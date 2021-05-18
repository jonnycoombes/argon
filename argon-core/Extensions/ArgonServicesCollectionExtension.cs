#region

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.OperationFilters;
using JCS.Argon.Services.Core;
using JCS.Argon.Services.VSP;
using JCS.Neon.Glow.Helpers.General;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

#endregion

namespace JCS.Argon.Extensions
{
    public static class ArgonServicesCollectionExtension
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext(typeof(ArgonServicesCollectionExtension));

        /// <summary>
        ///     Register the db context, optional branching here to allow for different connection strings based on the
        ///     currently configured environment
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="hostEnvironment"></param>
        public static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration config,
            IWebHostEnvironment hostEnvironment)
        {
            LogHelpers.LogMethodCall(_log);
            try
            {
                services.AddDbContext<SqlDbContext>(options =>
                {
                    options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                        sqlOptions => { sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); });
                    options.EnableDetailedErrors();
                });
            }
            catch (Exception ex)
            {
                LogHelpers.LogExceptionError(_log, ex);
                LogHelpers.LogError(_log, "Caught an exception whilst attempting to register Db context");
            }

            return services;
        }

        /// <summary>
        ///     Adds any argon-specific configuration elements into the IoC container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterArgonConfig(this IServiceCollection services, IConfiguration config)
        {
            LogHelpers.LogMethodCall(_log);
            services.AddOptions();
            services.Configure<ApiOptions>(config.GetSection(ApiOptions.ConfigurationSection));
            return services;
        }

        /// <summary>
        ///     Configures response compression.  Just basically uses the standard settings for Gzip and brotli
        /// </summary>
        /// <param name="services">The current <see cref="IServiceCollection" /></param>
        private static void ConfigureResponseCompression(IServiceCollection services)
        {
            LogHelpers.LogMethodCall(_log);
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
        ///     All Argon-specific services should be added to the IoC container here
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterArgonServices(this IServiceCollection services, IConfiguration config)
        {
            LogHelpers.LogMethodCall(_log);
            RegisterApiServices(services, config);
            RegisterCoreServices(services, config);
            RegisterHttpClientServices(services, config);
            ConfigureResponseCompression(services);
            return services;
        }

        /// <summary>
        ///     Registers a <see cref="HttpClient" /> for use within the storage layer.  By default this client is configured to
        ///     utilise the default credentials so that pass
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        private static void RegisterHttpClientServices(IServiceCollection services, IConfiguration config)
        {
            LogHelpers.LogMethodCall(_log);
            services.AddHttpClient<IVirtualStorageManager, VirtualStorageManager>().ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                });
        }

        /// <summary>
        ///     Do anything specific to controller bindings, Swagger configuration etc...
        ///     Specific alterations here to ignore null values in Json serialisation to
        ///     minimise payload size, and also to de-clutter responses.
        ///     in here.  In order to ensure that Swashbuckle generates automated documentation correctly,
        ///     we inject an <see cref="IOperationFilter" /> to modify/subvert the generation of parameters on the outgoing
        /// </summary>
        /// <param name="services">Current services collection</param>
        /// <param name="config"></param>
        private static void RegisterApiServices(IServiceCollection services, IConfiguration config)
        {
            LogHelpers.LogMethodCall(_log);
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<ArgonOperationFilter>();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Argon - Content Service Layer",
                    Version = $"v1 ({ReflectionHelpers.GetApplicationAssemblyVersion()})",
                    Description = $"Argon. (Build Version: {ReflectionHelpers.GetApplicationAssemblyVersion()})",
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
        ///     Register all application-specific services such as the VSP registry etc...
        ///     Note the differences between whether services are scoped (basically per-session) or
        ///     singleton
        /// </summary>
        /// <param name="services">The current services collection</param>
        /// <param name="config"></param>
        private static void RegisterCoreServices(IServiceCollection services, IConfiguration config)
        {
            LogHelpers.LogMethodCall(_log);
            LogHelpers.LogInformation(_log, "Registering a scoped VSP factory");
            services.AddSingleton<IVirtualStorageManager, VirtualStorageManager>();
            LogHelpers.LogInformation(_log, "Registering a scoped instance of IArchiveManager");
            services.AddScoped<IArchiveManager, ArchiveManager>();
            LogHelpers.LogInformation(_log, "Registering a scoped property group manager");
            services.AddScoped<IPropertyGroupManager, PropertyGroupManager>();
            LogHelpers.LogInformation(_log, "Registering a scoped constraint group manager");
            services.AddScoped<IConstraintGroupManager, ConstraintGroupManager>();
            LogHelpers.LogInformation(_log, "Registering a scoped item manager");
            services.AddScoped<IItemManager, ItemManager>();
            LogHelpers.LogInformation(_log, "Registering a scoped collection manager");
            services.AddScoped<ICollectionManager, CollectionManager>();
            LogHelpers.LogInformation(_log, "Registering db cache");
            services.AddScoped<IDbCache, DbCache>();
            LogHelpers.LogInformation(_log, "Registering global response exception handler");
            services.AddSingleton<IResponseExceptionHandler, ResponseExceptionHandler>();
        }
    }
}
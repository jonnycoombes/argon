using System;
using System.Collections.Generic;
using System.Linq;
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

        private void RegisterDbContext(IServiceCollection services)
        {
            Log.Information("Registering Db context");
            if (Environment.IsDevelopment())
            {
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

        private void RegisterCoreConfiguration(IServiceCollection services)
        {
            var apiConfiguration = new ApiConfiguration
            {
                VspConfigurationOptions = new VSPConfigurationOptions(Configuration)
            };
            services.AddSingleton(apiConfiguration);
        }
        
        /// <summary>
        /// Do anything specific to controller bindings, Swagger configuratino etc...
        /// in here
        /// </summary>
        /// <param name="services">Current services collection</param>
        protected void ConfigureApiServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "Argon", 
                    Version = "v1",
                    Description = "Glencore General Content Service Layer" });
            }); 
        }

        /// <summary>
        /// Register all application-specific services such as the VSP registry etc...
        /// </summary>
        /// <param name="services">The current services collection</param>
        protected void ConfigureCoreServices(IServiceCollection services)
        {
            services.AddScoped<IVSPFactory, VSPFactory>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterCoreConfiguration(services);
            RegisterDbContext(services);
            ConfigureApiServices(services);
            ConfigureCoreServices(services);
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
            }
            else
            {
                logger.LogInformation("Starting within a non-development environment");
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSerilogRequestLogging();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

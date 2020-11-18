using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private void RegisterCoreConfiguration(IServiceCollection services)
        {
            var coreConfiguration = new ApiConfiguration
            {
                VspConfigurationOptions = new VSPConfigurationOptions(Configuration)
            };
            services.AddSingleton(coreConfiguration);
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
            ConfigureApiServices(services);
            ConfigureCoreServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Argon v1"));
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

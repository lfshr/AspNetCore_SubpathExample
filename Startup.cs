using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace KubernetesSubpathExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });
        }

        /// <summary>
        /// Gets PathBase from configuration
        /// </summary>
        /// <returns>PathBase from configuration or string.Empty if not configured.</returns>
        string GetPathBase()
        {
            var pathBase = Configuration["PathBase"];

            if (string.IsNullOrEmpty(pathBase))
            {
                return string.Empty;
            }

            // Avoid double forward slash
            if (pathBase.Trim() == "/")
            {
                return string.Empty;
            }

            // If the path is specified but doesn't begin with a forward slash
            // throw an exception. Root path must begin with forward slash.
            // Better to explicitly require it than make it magic.
            if (pathBase.StartsWith("/") == false 
                && string.IsNullOrEmpty(pathBase) == false)
            {
                throw new Exception($"\"{pathBase}\" is not a valid PathBase. PathBase must start with /");
            }

            return pathBase;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Get pathbase from configuration, default to string.Empty
            var pathBase = GetPathBase();

            // Change the Path Base
            app.UsePathBase(pathBase);
            app.UseStaticFiles(pathBase);

            // Add swagger and tell SwaggerUI about the path base.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", "My API V1");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

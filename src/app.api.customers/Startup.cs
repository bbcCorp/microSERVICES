using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.HttpOverrides;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using AutoMapper;
using NLog.Web;
using NLog.Extensions.Logging;

using app.model;
using app.model.entities;
using app.data;
using app.api.customers.Model;

using app.api.customers.Extensions;

namespace app.api.customers
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
            // Add configuration so that configurations are available throughout the application
            services.AddSingleton(Configuration);

            services.AddAutoMapper(cfg =>
            {
                // Application entity model - view model mappings
                cfg.CreateMap<Customer, CustomerVM>().ReverseMap();
            });

            services.ConfigureLogger();

            services.ConfigureCors();

            services.AddMvc();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = 
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Use HTTPS in non-development environment
                app.UseHttpsRedirection();
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
        
            app.UseCors("CorsPolicy");

            // This is required to work with Reverse-proxies
            app.UseForwardedHeaders();

            app.UseMvc();
        }

    }
}

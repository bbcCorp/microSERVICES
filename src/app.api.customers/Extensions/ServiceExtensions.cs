using Microsoft.Extensions.DependencyInjection;
using app.model;
using app.data;
using app.api.customers.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using NLog.Extensions.Logging;

namespace app.api.customers.Extensions
{
    public static class ServiceExtensions
    {

        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }

        public static void ConfigureLogger(this IServiceCollection services)
        {            
            // Configure Logger
            var loggerFactory = new LoggerFactory();
            
            loggerFactory.AddNLog();
            loggerFactory.ConfigureNLog("nlog.config");            

            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }

    }
}
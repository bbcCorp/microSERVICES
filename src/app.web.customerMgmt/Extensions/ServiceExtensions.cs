using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using NLog.Web;
using NLog.Extensions.Logging;

namespace app.web.customerMgmt.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureLogger(this IServiceCollection services)
        {            
            // Configure Logger
            var loggerFactory = new LoggerFactory();
            
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties =true });
            NLog.LogManager.LoadConfiguration("nlog.config");            

            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }

    }
}                
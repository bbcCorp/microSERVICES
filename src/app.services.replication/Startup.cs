using System;
using System.IO;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using NLog;
using NLog.Extensions.Logging;

namespace app.services.replication
{
    public class Startup
    {
        private IServiceProvider  diServiceProvider;
        private IConfigurationRoot Configuration;
        private ILoggerFactory loggerFactory;

        private CancellationTokenSource cts;
        
        public Startup()
        {
            this.cts = new CancellationTokenSource();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables("MICROSERVICES_");

            this.Configuration = configBuilder.Build();

            this.Configure();  
            
        }


        public void Configure()
        {
            this.diServiceProvider = ConfigureServices();

            this.loggerFactory = diServiceProvider.GetService<ILoggerFactory>(); 

            //configure NLog
            loggerFactory.AddNLog(new NLogProviderOptions {                 
                CaptureMessageTemplates = true, 
                CaptureMessageProperties = true });

            NLog.LogManager.LoadConfiguration("nlog.config");

        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Add configuration so that configurations are available throughout the application
            //services.AddSingleton(Configuration);
            services.AddSingleton(this.Configuration);


            // Configure Logger
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
       
            return services.BuildServiceProvider();
            
        }

        public void Start()
        {                    
            var server = new AppReplicationServer(this.loggerFactory, this.Configuration);
                        
            server.StartServer(cts.Token);                
        }

        public void Stop()
        {
            // Stop the server loop
            cts.Cancel();
        }

    }
}

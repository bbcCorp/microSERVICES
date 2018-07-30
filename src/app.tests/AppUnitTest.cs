using Xunit;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace app.tests
{
    public class AppUnitTest
    {
        protected ILogger testLogger;
        protected LoggerFactory loggerFactory;
        protected IConfigurationRoot Configuration { get; private set; }

        public AppUnitTest()
        {
            var builder = new ConfigurationBuilder()
                // .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../.."))
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
                .AddJsonFile("testsettings.json");

            this.Configuration = builder.Build();

            this.loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            this.testLogger = loggerFactory.CreateLogger<AppUnitTest>();

        }
    }
}

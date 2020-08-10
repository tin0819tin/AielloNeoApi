using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Aiello_Restful_API.Models;
using Aiello_Restful_API.Config;
using Aiello_Restful_API.Controllers;
using NLog;
using NLog.Web;

namespace Aiello_Restful_API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
          
            try
            {
                logger.Debug("init main");
                Neo4jDriver.Register("bolt://localhost:7687", "neo4j", "password");
                //Neo4jDriver.Register("bolt://13.94.28.121:7687", "neo4j", "haL5hrM7EJAARny2");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    //logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                })
                .UseNLog();  // NLog: Setup NLog for Dependency injection
    }
}

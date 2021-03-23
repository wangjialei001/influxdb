using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace InfluxDB.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                var host = CreateHostBuilder(args).Build();
                try
                {
#if DEBUG
                    logger.Debug("init main");
                    Console.WriteLine("DEBUG");
#else

                    var msgDataService = host.Services.GetService(typeof(IMsgDataService)) as IMsgDataService;
                    msgDataService.ConsumRealData();
#endif
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Stopped program because of exception");
                    Console.WriteLine(ex.Message);
                    throw;
                }
                host.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseKestrel(opt => opt.Limits.MinRequestBodyDataRate = null);
                }).ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                }).UseNLog();  // NLog: Setup NLog for Dependency injection
    }
}

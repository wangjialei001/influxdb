using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InfluxDB.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            try
            {
#if DEBUG
                Console.WriteLine("DEBUG");
#else

                var msgDataService = host.Services.GetService(typeof(IMsgDataService)) as IMsgDataService;
                msgDataService.ConsumRealData();
#endif


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseKestrel(opt => opt.Limits.MinRequestBodyDataRate = null);
                });
    }
}

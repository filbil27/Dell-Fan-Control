using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dell_Fan_Control.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dell_Fan_Control
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true);
            var config = configBuilder.Build();
            CreateHostBuilder(args, config).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot configuration) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
                services.AddTransient<IMPIInteraction>();
                services.Configure<ApplicationOptions>(configuration.GetSection("ApplicationOptions"));
                services.Configure<IMPIOptions>(configuration.GetSection("IMPIOptions"));
                services.Configure<FanOptions>(configuration.GetSection("FanOptions"));
            });
    }
}

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
            var programData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dell Fan Control");
            CreateDirectory(programData);
            var configLoc = Path.Combine(programData, "Configuration");
            CreateDirectory(configLoc);
            var logLoc = Path.Combine(programData, "Logs");
            CreateDirectory(programData);

            var configBuilder = new ConfigurationBuilder()
               .SetBasePath(configLoc)
               .AddJsonFile("appsettings.json", optional: false)
#if DEBUG
               .AddJsonFile("appsettings.Development.json", optional: true)
#endif
               .AddJsonFile("fanLevels.Json", optional: false);
            var config = configBuilder.Build();
            CreateHostBuilder(args, config).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot configuration) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
                services.AddTransient<IMPIInteraction>();
                services.Configure<ApplicationOptions>(configuration.GetSection("ApplicationOptions"));
                services.Configure<IMPIOptions>(configuration.GetSection("IMPIOptions"));
                services.Configure<FanOptions>(configuration.GetSection("FanOptions"));
                services.Configure<FanLevels>(configuration);
            });


        private static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
        }
}
}

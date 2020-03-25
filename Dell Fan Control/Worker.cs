using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dell_Fan_Control
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMPIInteraction _iMPIInteraction;

        public Worker(ILogger<Worker> logger, IMPIInteraction iMPIInteraction)
        {
            _logger = logger;
            _iMPIInteraction = iMPIInteraction;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                _iMPIInteraction.GetTemperture();

                _iMPIInteraction.SetFan(10);
                await Task.Delay(5000, stoppingToken);
                _iMPIInteraction.SetFan(0);
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}

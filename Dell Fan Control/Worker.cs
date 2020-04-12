using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dell_Fan_Control.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dell_Fan_Control
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMPIInteraction _iMPIInteraction;
        private readonly FanOptions _fanOptions;
        private bool _fanManual = false;

        public Worker(ILogger<Worker> logger, IMPIInteraction iMPIInteraction, IOptions<FanOptions> fanOptions)
        {
            _logger = logger;
            _iMPIInteraction = iMPIInteraction;
            _fanOptions = fanOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetManualFan(true);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogTrace("Checking and adjusting fan speed");
                AdjustFan();
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            SetAutoFan();
            await Task.Delay(1000, cancellationToken);
        }

        private void SetManualFan(bool setToDefaultMax = false)
        {
            _iMPIInteraction.SetMannualFanControl();
            _fanManual = true;
            if (setToDefaultMax) { _iMPIInteraction.SetFan(_fanOptions.DefaultMaxPower); }
        }
        private void SetAutoFan()
        {
            _iMPIInteraction.SetAutomaticFanControl();
            _fanManual = false;
        }

        private void AdjustFan()
        {
            var tempertures = _iMPIInteraction.GetTemperture();
            var temperture = tempertures.OrderByDescending(s => s.Temp).FirstOrDefault();

            if (_fanManual)
            {
                if (temperture.Temp > _fanOptions.ReturnToAutomaticTemp) 
                {
                    _logger.LogDebug("Setting Fans back to Auto");
                    SetAutoFan(); 
                }
            }
            else
            {
                if (temperture.Temp < _fanOptions.ReturnToManualTemp) 
                {
                    _logger.LogDebug("Setting Fans to Manual");
                    SetManualFan(true); 
                }
            }
        }
    }
}

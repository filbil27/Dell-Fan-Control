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
        private readonly FanLevels _fanLevels;
        private bool _fanManual = false;
        private int _currentFanSpeed = -1;
        private int _highestTempLevel = -1;
        private int _lowestTempLevel = -1;

        public Worker(ILogger<Worker> logger, IMPIInteraction iMPIInteraction, IOptions<FanOptions> fanOptions, IOptions<FanLevels> fanLevels)
        {
            _logger = logger;
            _iMPIInteraction = iMPIInteraction;
            _fanOptions = fanOptions.Value;
            _fanLevels = fanLevels.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetManualFan(true);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogTrace("Checking and adjusting fan speed");
                AdjustFan(_fanLevels.TempertureMeasure);
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
            _currentFanSpeed = -1;
        }
        private Models.Temperture GetTemperture(Enum.TempertureMeasure measure)
        {
            var tempertures = _iMPIInteraction.GetTemperture();

            switch (measure)
            {
                case Enum.TempertureMeasure.Highest:
                    return tempertures.OrderByDescending(s => s.Temp).FirstOrDefault();

                case Enum.TempertureMeasure.Average:
                    var averageTemp = (int)tempertures.Average(s => s.Temp);
                    return new Models.Temperture(averageTemp);

                default:
                    return new Models.Temperture(500);
            }
        }

        private void AdjustFan(Enum.TempertureMeasure measure)
        {
            var temperture = GetTemperture(measure);

            if (temperture == null) { SetAutoFan(); }

            var level = _fanLevels.Levels.Where(o => o.Low < temperture.Temp && temperture.Temp < o.High).FirstOrDefault();

            if (level == null)
            {
                SetAutoFan();
            }
            else
            {
                if (!_fanManual) { _iMPIInteraction.SetMannualFanControl(); }
                if (_currentFanSpeed != level.Speed) 
                {
                    _currentFanSpeed = level.Speed;
                    _iMPIInteraction.SetFan(level.Speed); 
                }
            }
        }

        private int GetHighestTempertureLevel()
        {
            if (_highestTempLevel == -1) { _highestTempLevel = _fanLevels.Levels.OrderByDescending(o => o.High).First().High; }
            return _highestTempLevel;
        }

        private int GetLowestTempertureLevel()
        {
            if (_lowestTempLevel == -1) { _lowestTempLevel = _fanLevels.Levels.OrderBy(o => o.Low).First().Low; }
            return _lowestTempLevel;
        }
    }
}

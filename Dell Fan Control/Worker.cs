using System;
using System.Data;
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
        private readonly FanLevels _fanLevels;
        private DateTimeOffset _speedLastChanged;
        private bool _fanManual = false;
        private int _currentFanSpeed = -1;
        private int _highestTempLevel = -1;
        private int _lowestTempLevel = -1;

        public Worker(ILogger<Worker> logger, IMPIInteraction iMPIInteraction, IOptions<FanLevels> fanLevels)
        {
            _logger = logger;
            _iMPIInteraction = iMPIInteraction;
            _fanLevels = fanLevels.Value;
            _speedLastChanged = DateTimeOffset.MinValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetManualFan();

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

        private void SetManualFan()
        {
            _iMPIInteraction.SetMannualFanControl();
            _fanManual = true;
        }
        private void SetAutoFan()
        {
            _iMPIInteraction.SetAutomaticFanControl();
            _fanManual = false;
            _currentFanSpeed = -1;
            _speedLastChanged = DateTimeOffset.Now;
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
                if (CanAdjustFanSpeed(_currentFanSpeed, level.Speed, _fanLevels.MinimumTimeBeforeDropping)) { AdjustFan(level.Speed); }
            }
        }

        private void AdjustFan(int fanSpeed)
        {
            _currentFanSpeed = fanSpeed;
            _iMPIInteraction.SetFan(fanSpeed);
            _speedLastChanged = DateTimeOffset.Now;
        }

        /// <summary>
        /// If the worker is allowed to change the speed of the fan
        /// </summary>
        /// <param name="currentSpeed">Current speed of the fan</param>
        /// <param name="desiredSpeed">The value that the service wants to change the speed to</param>
        /// <param name="intervalBeforeFanChange">How long the service needs to wait before setting a lower fan speed. (An increase in fan speed will ingore this value) (In seconds)</param>
        /// <returns></returns>
        private bool CanAdjustFanSpeed(int currentSpeed, int desiredSpeed, int intervalBeforeFanChange)
        {
            if (desiredSpeed > currentSpeed)
            {
                _logger.LogDebug("Desired Speed is greater. Changing fan speed | Desired Speed: {DesiredSpeed} | Current Speed: {CurrentSpeed}", desiredSpeed, currentSpeed);
                return true;
            }
            else if ((desiredSpeed < currentSpeed) && (_speedLastChanged.AddSeconds(intervalBeforeFanChange) < DateTimeOffset.Now))
            {
                _logger.LogDebug("Desired Speed is less and Minimum Time Before Dropping has lasped. Changing fan speed | Desired Speed: {DesiredSpeed} | Current Speed: {CurrentSpeed} " +
                    "| Minimum Time Before Dropping: {MinimumTimeBeforeDropping}", desiredSpeed, currentSpeed, intervalBeforeFanChange);
                return true;
            }

            _logger.LogTrace("No requirement to change current fan speed | Desired Speed: {DesiredSpeed} | Current Speed: {CurrentSpeed} " +
                    "| Minimum Time Before Dropping: {MinimumTimeBeforeDropping}", desiredSpeed, currentSpeed, intervalBeforeFanChange);
            return false;
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

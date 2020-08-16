using Dell_Fan_Control.Models;
using Dell_Fan_Control.Options;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dell_Fan_Control
{
    public class IMPIInteraction
    {
        private readonly IMPIOptions _IMPIOptions;
        private readonly ApplicationOptions _applicationOptions;
        private static readonly string _commandTemplateSetFan = "-I lanplus -H {0} -U {1} -P \"{2}\" raw 0x30 0x30 0x02 0xff 0x{3}";
        private static readonly string _commandTemplateSetMannualFanControl = "-I lanplus -H {0} -U {1} -P \"{2}\" raw 0x30 0x30 0x01 0x00";
        private static readonly string _commandTemplateSetAutomaticFanControl = "-I lanplus -H {0} -U {1} -P \"{2}\" raw 0x30 0x30 0x01 0x01";
        private static readonly string _commandTemplateGetTemperature = "-I lanplus -H {0} -U {1} -P \"{2}\" sdr type temperature";
        public IMPIInteraction(IOptions<IMPIOptions> options, IOptions<ApplicationOptions> appOptions)
        {
            _IMPIOptions = options.Value;
            _applicationOptions = appOptions.Value;
        }

        public List<Temperture> GetTemperture()
        {
            var command = string.Format(_commandTemplateGetTemperature, _IMPIOptions.Host, _IMPIOptions.Username, _IMPIOptions.Password);
            var result = new List<Temperture> { };

            IssueCommand(command, out var output2);
            var tempOut = output2.Split("\n");
            foreach (var s in tempOut) 
            {
                if (!string.IsNullOrEmpty(s)) { result.Add(new Temperture(s)); }
            }

            return result;
        }

        public void SetFan(int powerPercentage)
        {
            if (100 < powerPercentage) { powerPercentage = 100; }
            else if (powerPercentage < 0) { powerPercentage = 0; }

            var powerPercentageHex = powerPercentage.ToString("x2");

            var command = string.Format(_commandTemplateSetFan, _IMPIOptions.Host, _IMPIOptions.Username, _IMPIOptions.Password, powerPercentageHex);

            IssueCommand(command, out var _);
        }

        private void IssueCommand(string command, out string consoleOutput)
        {
            //string fullCommand = string.Format("{0}impitool {1}", GetIMPILocation(), command);

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(GetIMPILocation(), command)
            {
                //WindowStyle = ProcessWindowStyle.Hidden,
                //FileName = "cmd.exe",
                //Arguments = string.Format("/C {0}", command),
                RedirectStandardOutput = true,
                //WorkingDirectory = GetIMPILocation()
            };
            process.StartInfo = startInfo;
            process.Start();
            consoleOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        private string GetIMPILocation() =>  _applicationOptions.IPMIToolLocation;
        public void SetAutomaticFanControl() => IssueCommand(string.Format(_commandTemplateSetAutomaticFanControl, _IMPIOptions.Host, _IMPIOptions.Username, _IMPIOptions.Password), out var _);
        public void SetMannualFanControl() => IssueCommand(string.Format(_commandTemplateSetMannualFanControl, _IMPIOptions.Host, _IMPIOptions.Username, _IMPIOptions.Password), out var _);
    }
}

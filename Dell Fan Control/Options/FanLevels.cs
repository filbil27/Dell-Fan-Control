using Dell_Fan_Control.Models;
using System.Collections.Generic;

namespace Dell_Fan_Control.Options
{
    public class FanLevels
    {
        public int MinimumTimeBeforeDropping { get; set; }
        public Enum.TempertureMeasure TempertureMeasure { get; set; }
        public List<FanRanges> Levels { get; set; }
    }
}

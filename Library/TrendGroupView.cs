using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class TrendGroupView
    {
        public int DeviceId { get; set; }
        public string? Device { get; set; }
        public string? Parameters { get; set; }
        public List<int>? ParameterIds { get; set; }
        public string? Type { get; set; }
    }
}

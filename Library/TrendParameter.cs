using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class TrendParameter
    {
        public int Id { get; set; }
        public bool Flag { get; set; }
        public SerialDevice? SerialDevice { get; set; }
        public int SerialDeviceId { get; set; }
        public SerialDeviceParameter? SerialDeviceParameter { get; set; }
        public int SerialDeviceParameterId { get; set; }
    }
}

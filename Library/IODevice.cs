using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkLibrary
{
    public class IODevice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IODeviceType DeviceType { get; set; }
        public string IpAddress { get; set; }
    }
    public enum IODeviceType
    {
        PLC,
        EthernetDevice,
        Gateway,
        CTAPI,
    }
}

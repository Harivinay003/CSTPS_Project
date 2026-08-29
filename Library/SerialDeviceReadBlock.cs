using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkLibrary
{
    public class SerialDeviceReadBlock
    {
        public int Id { get; set; }
        public SerialDeviceDriver Driver { get; set; }
        public int StartAddress { get; set; }
        public int Count { get; set; }
        public int DriverId { get; set; }
    }
}

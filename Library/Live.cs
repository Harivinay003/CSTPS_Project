using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class Live
    {
        public int Id { get; set; }

        public IODevice? Device { get; set; }
        public SerialDevice? SerialDevice { get; set; }
        public SerialDeviceParameter? Parameter { get; set; }
        public Tag? Tag { get; set; }
        public double? Value { get; set; }
    }
}

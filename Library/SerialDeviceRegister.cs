using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkLibrary
{
    public class SerialDeviceRegister
    {
        public int Id { get; set; }
        public SerialDeviceParameter Parameter { get; set; }
        public int RegisterAddress { get; set; }
        public DataType DataType { get; set; }
        public SerialDeviceDriver Driver { get; set; }
        public int DriverId { get; set; }
        public int ParameterId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkLibrary
{
    public class SerialDevice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SerialDeviceDriver Driver { get; set; }
        //Proxy for Driver
        public string DriverName
        {
            get
            {
                return Driver?.Name;
            }
        }
        public int UnitId { get; set; }
        public IODevice Gateway { get; set; }
        //proxy for Gateway
        public string GatwayName
        {
            get
            {
                return Gateway?.Name;
            }
        }
        public bool SwapRegs { get; set; }
        public int DriverId { get; set; }
        public int GatewayId { get; set; }
    }
}

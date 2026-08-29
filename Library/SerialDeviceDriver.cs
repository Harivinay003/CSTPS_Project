using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkLibrary
{
    public class SerialDeviceDriver
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public List<SerialDeviceRegister> ReadRegisters { get; set; }
        public List<SerialDeviceReadBlock> ReadBlocks { get; set; }
    }
}

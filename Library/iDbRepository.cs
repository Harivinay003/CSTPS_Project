using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VirtualEMS.Library;

namespace VirtualEMS.DataServices
{
    public interface iDbRepository
    {
        IEnumerable<AlarmTag> GetAlarmTags();
        IEnumerable<AlarmParameter> GetAlarmParameters();
        IEnumerable<FieldDevice> GetFieldDevices();
        IEnumerable<IODevice> GetIODevices();
        IEnumerable<SerialDevice> GetSerialDevices();
        IEnumerable<SerialDeviceDriver> GetSerialDeviceDrivers();
        IEnumerable<SerialDeviceParameter> GetSerialDeviceParameters();
        IEnumerable<SerialDeviceReadBlock> GetSerialDeviceReadBlocks();
        IEnumerable<SerialDeviceRegister> GetSerialDeviceRegisters();
        IEnumerable<Tag> GetTags();
        IEnumerable<TrendTag> GetTrendTags();
        IEnumerable<TrendParameter> GetTrendParameters();
        IEnumerable<Permits> GetPermits();
        IEnumerable<Group> GetGroup();
    }
}

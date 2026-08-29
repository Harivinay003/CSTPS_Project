using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VirtualEMS.Library;

namespace VirtualEMS.DataServices
{
    public class dbRepository : iDbRepository
    {
        private readonly AppDbContext context;

        public dbRepository(AppDbContext context)
        {
            this.context = context;
        }
        public IEnumerable<AlarmTag> GetAlarmTags()
        {
            return context.AlarmTags;
        }
        public IEnumerable<AlarmParameter> GetAlarmParameters()
        {
            return context.AlarmParameters;
        }
        public IEnumerable<FieldDevice> GetFieldDevices()
        {
            return context.FieldDevices.Include(f=>f.IODevice).Include(f=>f.Tags);
        }

        public IEnumerable<IODevice> GetIODevices()
        {
            return context.IODevices;
        }
        
        public IEnumerable<SerialDevice> GetSerialDevices()
        {
            return context.SerialDevices;
        }

        public IEnumerable<SerialDeviceDriver> GetSerialDeviceDrivers()
        {
            return context.SerialDeviceDrivers;
        }

        public IEnumerable<SerialDeviceParameter> GetSerialDeviceParameters()
        {
            return context.SerialDeviceParameters;
        }

        public IEnumerable<SerialDeviceReadBlock> GetSerialDeviceReadBlocks()
        {
            return context.SerialDeviceReadBlocks;
        }

        public IEnumerable<SerialDeviceRegister> GetSerialDeviceRegisters()
        {
            return context.SerialDeviceRegisters;
        }

        public IEnumerable<Tag> GetTags()
        {
            return context.Tags;
        }
        public IEnumerable<TrendTag> GetTrendTags()
        {
            return context.TrendTags
                .Include(t => t.Tag)
                .Include(t => t.FieldDevice);
        }
        public IEnumerable<TrendParameter> GetTrendParameters()
        {
            return context.TrendParameters
                .Include(p => p.SerialDeviceParameter)
                .Include(p => p.SerialDevice);
        }
        public IEnumerable<Permits> GetPermits()
        {
            return context.Permits;
        }
        public IEnumerable<Group> GetGroup()
        {
            return context.Group;
        }
        // User related methods
        public VirtualEMS.Library.User GetUserByUsername(string username)
        {
            return context.Set<VirtualEMS.Library.User>().FirstOrDefault(u => u.Username == username);
        }

        public void UpdateUser(VirtualEMS.Library.User user)
        {
            context.Set<VirtualEMS.Library.User>().Update(user);
            context.SaveChanges();
        }
    }
}

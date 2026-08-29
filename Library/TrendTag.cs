using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class TrendTag
    {
        public int Id { get; set; }
        public FieldDevice? FieldDevice { get; set; }
        public int FieldDeviceId { get; set; }
        public int TagId { get; set; }
        public bool Flag { get; set; }
        public Tag? Tag { get; set; }    
    }
}

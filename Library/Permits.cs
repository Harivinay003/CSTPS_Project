using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class Permits
    {
        public int Id { get; set; }
        public FieldDevice? FieldDevice { get; set; }
        public int FieldDeviceId { get; set; }
        public Tag? Tag { get; set; }
        public int TagId { get; set; }
        [NotMapped]
        public PermitType Type { get; set; }
    }
    public enum PermitType
    {
        Permit ,
        Electrical,
        Mechanical1,
        Mechanical2,
        Operation
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class Group
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        //public bool Mains { get; set; }
        //public bool Subs { get; set; }
        //public List<GroupItem> GroupItems { get; set; }
    }
    public class GroupItem
    {
        public int Id { get; set; }
        public Group? Group { get; set; }
        public int GroupId { get; set; }
        public SerialDevice? SerialDevice { get; set; }
        public FieldDevice? FieldDevice { get; set; }
        public GroupOperationType OperationType { get; set; }

    }
    public enum GroupOperationType
    {
        Add = 1,
        Subtract = 2
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class  AlarmTag
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public FieldDevice? FieldDevice { get; set; }
        public int FieldDeviceId { get; set; }
        public Tag? Tag { get; set; }
        public int TagId { get; set; }
        public Single? LowSetPoint { get; set; }
        public Single? HighSetPoint { get; set; }
        public bool? Critical { get; set; }
        public bool? LogOn { get; set; }
        public int? Group {  get; set; }
        public EventCategory? Category { get; set; }
        public bool? Changed { get; set; }
    }
    public class AlarmParameter
    {
        public int Id { get; set; }
        public string?  Name { get; set; }
        public string? Description { get; set; }
        public SerialDevice? SerialDevice { get; set; }
        public int SerialDeviceId { get; set; }
        public SerialDeviceParameter? SerialDeviceParameter { get; set; }
        public int SerialDeviceParameterId { get; set; }
        public Single? LowSetPoint { get; set; }
        public Single? HighSetPoint { get; set; }
        public bool? Critical { get; set; }
        public bool? LogOn { get; set; }
        public int? Group { get; set; }
        public EventCategory? Category { get; set; }
        public bool? Changed { get; set; }
    }
    public enum EventCategory
    {
        Alarm,
        Event,
        Trip
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
        public class AlarmViewModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? Device { get; set; }
            public string? Parameter { get; set; }
            public float? LowSetPoint { get; set; }
            public float? HighSetPoint { get; set; }
            public bool Critical { get; set; }
            public bool? Changed { get; set; }
            public string? LogOn { get; set; }
            public string? Type { get; set; } // Tag or Parameter
            public String? Group { get; set; }
            public EventCategory Category { get; set; }
        }
}

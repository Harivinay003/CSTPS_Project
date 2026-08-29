using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkLibrary
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DataType Type { get; set; }
        public FieldDevice FieldDevice { get; set; }
        public IODevice Device { get; set; }
        public string Units { get; set; }
        public int Address { get; set; }
        public int Bit { get; set; }
        public string Formula { get; set; }
        public Single SP_Min { get; set; }
        public Single SP_Max { get; set; }
        public Single Scale_Min { get; set; }
        public Single Scale_Max { get; set; }
        public StorageType StorageType { get; set; }
    }
    public enum StorageType
    {
        NONE,
        CYCLIC,
        DELTA
    }
    public enum DataType
    {
        INT,
        DINT,
        UINT,
        REAL,
        BOOL,
        WBOOL,
        DWORD,
        WORD,
        INT64,
        UDINT
    }
}

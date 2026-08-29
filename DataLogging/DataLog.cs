using Azure;
using FluentModbus;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace DataLogging
{
    public class DataLog : IDisposable, IHostedService
    {
        public static List<AlarmTag> AlarmTags = new List<AlarmTag>();
        public static List<AlarmParameter> AlarmParameters = new List<AlarmParameter>();
        //public static List<Category> Categories = new List<Category>();
        public static List<FieldDevice> FieldDevices = new List<FieldDevice>();
        public static List<IODevice> IODevices = new List<IODevice>();
        public static List<SerialDevice> SerialDevices = new List<SerialDevice>();
        public static List<SerialDeviceDriver> SerialDeviceDrivers = new List<SerialDeviceDriver>();
        public static List<SerialDeviceParameter> SerialDeviceParameters = new List<SerialDeviceParameter>();
        public static List<SerialDeviceReadBlock> SerialDeviceReadBlocks = new List<SerialDeviceReadBlock>();
        public static List<SerialDeviceRegister> SerialDeviceRegisters = new List<SerialDeviceRegister>();
        public static List<Tag> Tags = new List<Tag>();
        public static List<TrendTag> TrendTags = new List<TrendTag>();
        public static List<TrendParameter> TrendParameters = new List<TrendParameter>();
        public static List<Permits> Permits = new List<Permits>();

        System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();
        string SoundFile = AppDomain.CurrentDomain.BaseDirectory + "/Conf/BEEP.wav";
        bool buzzer = false;
        string DataConString, ConfigConString;

        List<Timer> Timers = new List<Timer>();

        // cache of databases already created in this process to avoid repeated CREATE calls
        private readonly HashSet<string> _createdDatabases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Timer? _monthlyDbTimer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        // keep static lists as before, timers etc.

        public DataLog(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            ConfigConString = _configuration.GetConnectionString("ConfigDBConnString");
            DataConString = _configuration.GetConnectionString("DataDBConnString");
            soundPlayer.SoundLocation = SoundFile;
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.WriteLog("Exception: " + (e.ExceptionObject as Exception).Message);
        }

        // Ensure monthly database exists and basic schema required by InsertAnalogValue is present.
        private void EnsureMonthlyDatabaseExists(DateTime time)
        {
            var dbName = time.ToString("MMM-yyyy", CultureInfo.InvariantCulture);

            // fast path: if created already during process lifetime skip
            if (_createdDatabases.Contains(dbName))
                return;

            var masterConn = DataConString + $";Initial Catalog=master;";
            try
            {
                using (var con = new SqlConnection(masterConn))
                {
                    con.Open();
                    // create database if not exists
                    string createDbSql = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}];";
                    using (var cmd = new SqlCommand(createDbSql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // create required tables inside the new database if they do not exist
                    // NOTE: AnalogData now uses a composite primary key (DevId, SerDevId, ParamId, LogTime)
                    // and does NOT use an identity Id column.
                    string createTables = $@"
USE [{dbName}];

IF OBJECT_ID('dbo.AnalogData','U') IS NULL
BEGIN
    CREATE TABLE dbo.AnalogData(
        DevId INT NOT NULL,
        SerDevId INT NOT NULL,
        ParamId INT NOT NULL,
        LogTime DATETIME NOT NULL,
        Value REAL NULL,
        CONSTRAINT PK_AnalogData PRIMARY KEY (DevId, SerDevId, ParamId, LogTime)
    );
END

IF OBJECT_ID('dbo.DigitalData','U') IS NULL
BEGIN
    CREATE TABLE dbo.DigitalData(
        TagID INT NOT NULL,
        OnTime DATETIME NOT NULL,
        OffTime DATETIME NULL,
        CONSTRAINT PK_DigitalData PRIMARY KEY (TagID,OnTime)
    );
END

IF OBJECT_ID('dbo.AlarmsData','U') IS NULL
BEGIN
    CREATE TABLE dbo.AlarmsData(
        AlarmId INT NOT NULL,
        AlarmType INT NOT NULL,
        LogTime DATETIME NOT NULL,
        Comment NVARCHAR(255) NULL,
        Value REAL NULL,
        ResetTime DATETIME NULL,
        Acknowledged bit NULL,
        CONSTRAINT PK_AlarmsData PRIMARY KEY (AlarmId, AlarmType, LogTime)
    );
END

IF OBJECT_ID('dbo.PermitsData','U') IS NULL
BEGIN
    CREATE TABLE dbo.PermitsData(
        PermitId INT NOT NULL,
        LogTime DATETIME NOT NULL,
        Comment NVARCHAR(255) NULL,
        ResetTime DATETIME NULL,
        CONSTRAINT PK_PermitsData PRIMARY KEY (PermitId, LogTime)
    );
END
";
                    using (var cmd = new SqlCommand(createTables, con))
                    {
                        cmd.CommandTimeout = 60;
                        cmd.ExecuteNonQuery();
                    }

                    _createdDatabases.Add(dbName);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog($"EnsureMonthlyDatabaseExists('{dbName}') failed: {ex.Message}");
            }
        }

        // Schedule a timer to create next month's database at midnight on the 1st day
        private void ScheduleMonthlyDatabaseCreation()
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime firstOfNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                DateTime nextRun = new DateTime(firstOfNextMonth.Year, firstOfNextMonth.Month, 1, 0, 0, 0);
                TimeSpan due = nextRun - now;
                if (due < TimeSpan.Zero) due = TimeSpan.FromSeconds(10);

                // dispose existing timer if any
                _monthlyDbTimer?.Dispose();
                _monthlyDbTimer = new Timer(_ =>
                {
                    try
                    {
                        EnsureMonthlyDatabaseExists(DateTime.Now);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog("Monthly DB timer error: " + ex.Message);
                    }
                    finally
                    {
                        // reschedule for next month
                        ScheduleMonthlyDatabaseCreation();
                    }
                }, null, due, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                Log.WriteLog("ScheduleMonthlyDatabaseCreation failed: " + ex.Message);
            }
        }

        private void InsertAlarm(int AlarmId, int AlarmType, string comment, float Value, DateTime time)
        {
            string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)}";
            try
            {
                // ensure monthly DB and schema exist before inserting
                EnsureMonthlyDatabaseExists(time);

                using (SqlConnection con = new SqlConnection(dconString))
                {
                    try
                    {
                        con.Open();
                        string insSql = @"IF NOT EXISTS(
                                            SELECT 1 FROM AlarmsData
                                            WHERE AlarmId = @AlarmId AND AlarmType = @AlarmType AND ResetTime IS NULL
                                        )
                                        BEGIN
                                            INSERT INTO AlarmsData (AlarmId, AlarmType, LogTime, Comment, Value, Acknowledged)
                                            VALUES (@AlarmId, @AlarmType, @LogTime, @Comment, @Value,0);
                                        END
                                        ";

                        using (var insCommand = new SqlCommand(insSql, con))
                        {
                            insCommand.Parameters.Add("@AlarmId", SqlDbType.Int).Value = AlarmId;
                            insCommand.Parameters.Add("@AlarmType", SqlDbType.Int).Value = AlarmType;
                            insCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                            insCommand.Parameters.Add("@Comment", SqlDbType.VarChar).Value = comment;
                            insCommand.Parameters.Add("@Value", SqlDbType.Float).Value = Value;
                            insCommand.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog("InsertAlarm: AlarmId: " + AlarmId + " Type - " + AlarmType + " - " + ex.Message);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("Insert Alarm error: " + ex.Message);
            }
        }
        private void UpdateAlarm(int AlarmId, int AlarmType, DateTime time)
        {
            string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)}";
            try
            {
                // ensure monthly DB and schema exist before inserting
                EnsureMonthlyDatabaseExists(time);

                using (SqlConnection con = new SqlConnection(dconString))
                {
                    try
                    {
                        con.Open();

                        string updateSql = @"UPDATE AlarmsData SET ResetTime = @ResetTime
                                        WHERE AlarmId = @AlarmId AND AlarmType = @AlarmType AND ResetTime IS NULL";
                        using (var delCommand = new SqlCommand(updateSql, con))
                        {
                            delCommand.Parameters.Add("@AlarmId", SqlDbType.Int).Value = AlarmId;
                            delCommand.Parameters.Add("@AlarmType", SqlDbType.Int).Value = AlarmType;
                            delCommand.Parameters.Add("@ResetTime", SqlDbType.DateTime).Value = time;
                            delCommand.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog("UpdateAlarm: AlarmId: " + AlarmId + " Type - " + AlarmType + " - " + ex.Message);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("UpdateAlarm error: " + ex.Message);
            }
        }

        //private void InsertAlarm(IODevice device, SerialDevice? serDev, int ParamId, int v, DateTime time, Single Value, bool Critical)
        //{
        //    int IODevId = device.Id;
        //    int SerDevId = serDev == null ? 0 : serDev.Id;
        //    string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)}";
        //    try
        //    {
        //        // ensure monthly DB and schema exist before inserting
        //        EnsureMonthlyDatabaseExists(time);

        //        using (SqlConnection con = new SqlConnection(dconString))
        //        {
        //            try
        //            {
        //                con.Open();
        //                string comment = v == 0 ? "Low Set Point Alarm" : "High Set Point Alarm";

        //                // Using parameterized IF NOT EXISTS -> INSERT.
        //                // Composite primary key on (DevId, SerDevId, ParamId, LogTime) guarantees uniqueness.
        //                string insSql = @"IF NOT EXISTS(
        //                                    SELECT 1 FROM AlarmsData
        //                                    WHERE DevId = @DevId AND SerDevId = @SerDevId AND ParamId = @ParamId and ResetTime IS NULL
        //                                )
        //                                BEGIN
        //                                    INSERT INTO AlarmsData (DevId, SerDevId, ParamId, LogTime, Comment, Value, Critical)
        //                                    VALUES (@DevId, @SerDevId, @ParamId, @LogTime,@Comment, @Value, @Critical);
        //                                END
        //                                ";

        //                using (var insCommand = new SqlCommand(insSql, con))
        //                {
        //                    insCommand.Parameters.Add("@DevId", SqlDbType.Int).Value = IODevId;
        //                    insCommand.Parameters.Add("@SerDevId", SqlDbType.Int).Value = SerDevId;
        //                    insCommand.Parameters.Add("@ParamId", SqlDbType.Int).Value = ParamId;
        //                    insCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
        //                    insCommand.Parameters.Add("@Comment", SqlDbType.VarChar).Value = comment;
        //                    insCommand.Parameters.Add("@Value", SqlDbType.Float).Value = Value;
        //                    insCommand.Parameters.Add("@Critical", SqlDbType.Bit).Value = Critical;
        //                    insCommand.ExecuteNonQuery();
        //                }

        //                con.Close();
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.WriteLog("Insert: TagId: " + ParamId + " Value - " + Value + " - " + ex.Message);
        //                return;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteLog("InsertAlarm error: " + ex.Message);
        //    }
        //}

        private void InsertAnalogValue(IODevice device, SerialDevice? sDev, int ParamId, double Value, DateTime time)
        {
            int IODevId = device.Id;
            int SerDevId = sDev == null ? 0 : sDev.Id;
            string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)}";

            try
            {
                // ensure monthly DB and schema exist before inserting
                EnsureMonthlyDatabaseExists(time);

                using (SqlConnection con = new SqlConnection(dconString))
                {
                    try
                    {
                        con.Open();
                        DateTime hTime = time.AddSeconds(-time.Second).AddMilliseconds(-time.Millisecond);

                        // Using parameterized IF NOT EXISTS -> INSERT.
                        // Composite primary key on (DevId, SerDevId, ParamId, LogTime) guarantees uniqueness.
                        string insSql = @"
                                        IF NOT EXISTS(
                                            SELECT 1 FROM AnalogData
                                            WHERE DevId = @DevId AND SerDevId = @SerDevId AND ParamId = @ParamId AND LogTime = @LogTime1
                                        )
                                        BEGIN
                                            INSERT INTO AnalogData (DevId, SerDevId, ParamId, LogTime, Value)
                                            VALUES (@DevId, @SerDevId, @ParamId, @LogTime, @Value);
                                        END
                                        ";

                        using (var insCommand = new SqlCommand(insSql, con))
                        {
                            insCommand.Parameters.Add("@DevId", SqlDbType.Int).Value = IODevId;
                            insCommand.Parameters.Add("@SerDevId", SqlDbType.Int).Value = SerDevId;
                            insCommand.Parameters.Add("@ParamId", SqlDbType.Int).Value = ParamId;
                            insCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = hTime;
                            insCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = hTime;
                            insCommand.Parameters.Add("@Value", SqlDbType.Float).Value = Value;
                            insCommand.ExecuteNonQuery();
                        }

                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog("Insert: TagId: " + ParamId + " Value - " + Value + " - " + ex.Message);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("InsertAnalogValue error: " + ex.Message);
            }
        }

       private void InsertTrendValue(IODevice device, SerialDevice? sDev, int ParamId, double Value, DateTime time)
        {
            int IODevId = device.Id;
            int SerDevId = sDev == null ? 0 : sDev.Id;
            string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)}";

            try
            {
                // ensure monthly DB and schema exist before inserting
                EnsureMonthlyDatabaseExists(time);

                using (SqlConnection con = new SqlConnection(dconString))
                {
                    try
                    {
                        con.Open();
                        DateTime hTime = time.AddMilliseconds(-time.Millisecond);

                        // Using parameterized IF NOT EXISTS -> INSERT.
                        // Composite primary key on (DevId, SerDevId, ParamId, LogTime) guarantees uniqueness.
                        string insSql = @"INSERT INTO AnalogData (DevId, SerDevId, ParamId, LogTime, Value)
                                            VALUES (@DevId, @SerDevId, @ParamId, @LogTime, @Value);";

                        using (var insCommand = new SqlCommand(insSql, con))
                        {
                            insCommand.Parameters.Add("@DevId", SqlDbType.Int).Value = IODevId;
                            insCommand.Parameters.Add("@SerDevId", SqlDbType.Int).Value = SerDevId;
                            insCommand.Parameters.Add("@ParamId", SqlDbType.Int).Value = ParamId;
                            insCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = hTime;
                            insCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = hTime;
                            insCommand.Parameters.Add("@Value", SqlDbType.Float).Value = Value;
                            insCommand.ExecuteNonQuery();
                        }

                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog("Insert: TagId: " + ParamId + " Value - " + Value + " - " + ex.Message);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("InsertTrendValue error: " + ex.Message);
            }
        }


        private void DoWork(object state)
        {

            DateTime time = DateTime.Now;

            IODevice device = (IODevice)state;
            IODeviceType deviceType = device.DeviceType;
            string IpAddress = device.IpAddress;
            switch (deviceType)
            {
                case IODeviceType.Gateway:
                    List<SerialDevice> serialDevs = SerialDevices.Where(s => s.Gateway == device).ToList();
                    try
                    {
                        using (ModbusTcpClient modbusClient = new ModbusTcpClient())
                        //EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient();
                        {
                            try
                            {
                                modbusClient.Connect(new IPEndPoint(IPAddress.Parse(IpAddress), 502));
                                //modbusClient.Connect(IpAddress, 502);
                            }
                            catch (Exception ex)
                            {
                                Log.WriteLog("Can't connect to " + device.Name);
                                Log.WriteLog(ex.Message);
                            }
                            if (modbusClient.IsConnected)
                            //if (modbusClient.Connected)
                            {
                                foreach (SerialDevice SerDev in serialDevs)
                                {
                                    SerialDeviceDriver driver = SerDev.Driver;
                                    int unitIdentifier = Convert.ToByte(SerDev.UnitId);
                                    //modbusClient.UnitIdentifier = Convert.ToByte(dev.UnitId);
                                    List<SerialDeviceReadBlock> readBlocks = driver.ReadBlocks;
                                    foreach (SerialDeviceReadBlock block in readBlocks)
                                    {
                                        int start = block.StartAddress;
                                        int count = block.Count * 2;
                                        int end = block.StartAddress + block.Count - 1;
                                        try
                                        {
                                            List<SerialDeviceRegister> registers = driver.ReadRegisters.Where(reg => reg.RegisterAddress >= start && reg.RegisterAddress <= end).ToList();
                                            var readbytes = modbusClient.ReadHoldingRegisters<byte>(unitIdentifier, start, count).ToArray();

                                            //var readreg = modbusClient.ReadHoldingRegisters(block.StartAddress, block.Count);

                                            foreach (SerialDeviceRegister reg in registers)
                                            {
                                                int address = reg.RegisterAddress;
                                                int paramId = reg.Parameter.Id;
                                                DataType dataType = reg.DataType;
                                                int buffer = (address - start) * 2;
                                                try
                                                {
                                                    switch (dataType)
                                                    {
                                                        case DataType.REAL:
                                                            byte[] bytes = new byte[4];
                                                            if (SerDev.SwapRegs)
                                                            {
                                                                bytes = new byte[] { readbytes[buffer + 3], readbytes[buffer + 2], readbytes[buffer + 1], readbytes[buffer + 0] };
                                                            }
                                                            else
                                                            {
                                                                bytes = new byte[] { readbytes[buffer + 1], readbytes[buffer], readbytes[buffer + 3], readbytes[buffer + 2] };
                                                            }

                                                            Single singleValue = BitConverter.ToSingle(bytes, 0);

                                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                                            {
                                                                con.Open();
                                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = " + SerDev.Id + " and ParamId = " + paramId + ") " +
                                                                    "BEGIN UPDATE Live SET Time = @LogTime, Value = " + singleValue + " " +
                                                                    "WHERE DevId = " + device.Id + " and SerDevId = " + SerDev.Id + " and ParamId = " + paramId + "; " +
                                                                    "END " +
                                                                    "ELSE " +
                                                                    "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                                    "VALUES (" + device.Id + ", " + SerDev.Id + " , " + paramId + ", @LogTime1, " + singleValue + ");" +
                                                                    "END";


                                                                //string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                                //    "VALUES (" + device.Id + ", " + SerDev.Id + " , " + paramId + ", @LogTime, " + singleValue + ") " +
                                                                //    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + singleValue + ";";

                                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                                try
                                                                {
                                                                    updateCommand.ExecuteNonQuery();
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    Log.WriteLog("Gateway Update: " + ex.Message);
                                                                }
                                                                con.Close();
                                                            }

                                                            if(TrendParameters.FirstOrDefault(p=> p.SerialDeviceParameterId == paramId && p.SerialDeviceId == SerDev.Id) != null)
                                                            {
                                                                InsertTrendValue(device, SerDev, paramId, singleValue, time);
                                                            }

                                                            {
                                                                InsertAnalogValue(device, SerDev, paramId, singleValue, time);
                                                                //check for alarms
                                                                AlarmParameter parameter = AlarmParameters.FirstOrDefault(p => p.SerialDeviceParameterId == paramId && p.SerialDeviceId == SerDev.Id);
                                                                if (parameter != null)
                                                                {
                                                                    //Insert or Update Alarm
                                                                    if (parameter.LowSetPoint != null && singleValue < parameter.LowSetPoint)
                                                                    {
                                                                        InsertAlarm(parameter.Id, 0, "Low Set Point Alarm", singleValue, time);
                                                                    }
                                                                    else if (parameter.HighSetPoint != null && singleValue > parameter.HighSetPoint)
                                                                    {
                                                                        InsertAlarm(parameter.Id, 0, "High Set Point Alarm", singleValue, time);
                                                                    }
                                                                    else
                                                                    {
                                                                        // clear existing alarm if value back to normal range
                                                                        UpdateAlarm(parameter.Id, 0, time);
                                                                    }
                                                                    if (parameter.LowSetPoint != null && singleValue < parameter.LowSetPoint) ;
                                                                }
                                                            }
                                                            break;
                                                        case DataType.INT64:
                                                            //Int64 int64Value = BitConverter.ToInt64(new byte[] { readbytes[buffer + 1], readbytes[buffer + 0], readbytes[buffer + 3], readbytes[buffer + 2], readbytes[buffer + 5], readbytes[buffer + 4], readbytes[buffer + 7], readbytes[buffer + 6] }, 0);
                                                            Single real1 = BitConverter.ToSingle(new byte[] { readbytes[buffer + 7], readbytes[buffer + 6], readbytes[buffer + 5], readbytes[buffer + 4] });
                                                            Single real2 = BitConverter.ToSingle(new byte[] { readbytes[buffer + 1], readbytes[buffer], readbytes[buffer + 3], readbytes[buffer + 2] });
                                                            double int64Value = real2 * 4294967.296 + (real1 / 1000);
                                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                                            {
                                                                con.Open();
                                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = " + SerDev.Id + " and ParamId = " + paramId + ") " +
                                                                    "BEGIN UPDATE Live SET Time = @LogTime, Value = " + int64Value + " " +
                                                                    "WHERE DevId = " + device.Id + " and SerDevId = " + SerDev.Id + " and ParamId = " + paramId + "; " +
                                                                    "END " +
                                                                    "ELSE " +
                                                                    "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                                    "VALUES (" + device.Id + ", " + SerDev.Id + " , " + paramId + ", @LogTime1, " + int64Value + ");" +
                                                                    "END";


                                                                string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                                    "VALUES (" + device.Id + ", " + SerDev.Id + " , " + paramId + ", @LogTime, " + int64Value + ") " +
                                                                    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + int64Value + ";";

                                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                                try
                                                                {
                                                                    updateCommand.ExecuteNonQuery();
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    Log.WriteLog("Gateway Update: " + ex.Message);
                                                                }
                                                                con.Close();
                                                            }
                                                            if (TrendParameters.FirstOrDefault(p => p.SerialDeviceParameterId == paramId && p.SerialDeviceId == SerDev.Id) != null)
                                                            {
                                                                InsertTrendValue(device, SerDev, paramId, int64Value, time);
                                                            }
                                                            {
                                                                InsertAnalogValue(device, SerDev, paramId, int64Value, time);

                                                                //check for alarms
                                                                AlarmParameter parameter = AlarmParameters.FirstOrDefault(p => p.SerialDeviceParameterId == paramId && p.SerialDeviceId == SerDev.Id);
                                                                if (parameter != null)
                                                                {
                                                                    if (parameter.LowSetPoint != null && int64Value < parameter.LowSetPoint)
                                                                    {
                                                                        InsertAlarm(parameter.Id, 0, "Low Set Point Alarm", (float)int64Value, time);
                                                                    }
                                                                    else if (parameter.HighSetPoint != null && int64Value > parameter.HighSetPoint)
                                                                    {
                                                                        InsertAlarm(parameter.Id, 0, "High Set Point Alarm", (float)int64Value, time);
                                                                    }
                                                                    else
                                                                    {
                                                                        // clear existing alarm if value back to normal range
                                                                        UpdateAlarm(parameter.Id, 0, time);
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    //Log.WriteLog("Read error from serial device " + SerDev.Name + ", block: " + block.StartAddress + ", Count " + count);
                                                    //Log.WriteLog(ex.Message);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //Log.WriteLog("Read from serial device " + SerDev.Name + ", block: " + block.StartAddress + ", Count " + count + ", id " + SerDev.UnitId);
                                            //Log.WriteLog(ex.Message);
                                        }
                                    }
                                }
                                modbusClient.Disconnect();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog("Gateway eroor: " + ex.Message);
                    }
                    break;

                case IODeviceType.PLC:
                case IODeviceType.EthernetDevice:
                    List<Tag> tags = Tags.Where(t => t.Device == device).ToList();
                    using (ModbusTcpClient modbusClient = new ModbusTcpClient())
                    {
                        try
                        {
                            modbusClient.Connect(new IPEndPoint(IPAddress.Parse(IpAddress), 502));
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog("Can't connect to " + device.Name);
                            Log.WriteLog(ex.Message);
                        }
                        if (modbusClient.IsConnected)
                        {
                            int unitIdentifier = 0x03;
                            foreach (Tag tag in tags)
                            {
                                DataType type = tag.Type;
                                try
                                {
                                    switch (type)
                                    {
                                        case DataType.INT:
                                            var intbytes = modbusClient.ReadHoldingRegisters<byte>(unitIdentifier, tag.Address, 2);
                                            short intValue = BitConverter.ToInt16(new byte[] { intbytes[1], intbytes[0] }, 0);
                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                            {
                                                con.Open();
                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + ") " +
                                                            "BEGIN UPDATE Live SET Time = @LogTime, Value = " + intValue + " " +
                                                            "WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + "; " +
                                                            "END " +
                                                            "ELSE " +
                                                            "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                            "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime1, " + intValue + ");" +
                                                            "END";

                                                string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                    "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + intValue + ") " +
                                                    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + intValue + ";";

                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                try
                                                {
                                                    updateCommand.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("Integer Update: " + ex.Message);
                                                }
                                                con.Close();
                                            }
                                            if (TrendTags.FirstOrDefault(p => p.TagId == tag.Id && p.FieldDevice.IODeviceId == device.Id) != null)
                                            {
                                                InsertTrendValue(device, null, tag.Id, intValue, time);
                                            }
                                            {
                                                //Insert
                                                InsertAnalogValue(device, null, tag.Id, intValue, time);

                                                //check for alarms
                                                AlarmTag atag = AlarmTags.FirstOrDefault(p => p.FieldDevice.IODevice.Id == device.Id && p.TagId == tag.Id);
                                                if (atag != null)
                                                {
                                                    if (atag.LowSetPoint != null && intValue < atag.LowSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id,1, "Low Set Point Alarm", (float)intValue, time);
                                                    }
                                                    else if (atag.HighSetPoint != null && intValue > atag.HighSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "High Set Point Alarm", (float)intValue, time);
                                                    }
                                                    else
                                                    {
                                                        UpdateAlarm(atag.Id, 1, time);
                                                    }
                                                }
                                            }
                                            break;
                                        case DataType.UINT:
                                            var uintbytes = modbusClient.ReadHoldingRegisters<byte>(unitIdentifier, tag.Address, 2);
                                            UInt16 uintValue = BitConverter.ToUInt16(new byte[] { uintbytes[1], uintbytes[0] }, 0);
                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                            {
                                                con.Open();
                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + ") " +
                                                            "BEGIN UPDATE Live SET Time = @LogTime, Value = " + uintValue + " " +
                                                            "WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + "; " +
                                                            "END " +
                                                            "ELSE " +
                                                            "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                            "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime1, " + uintValue + ");" +
                                                            "END";

                                                string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                    "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + uintValue + ") " +
                                                    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + uintValue + ";";

                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                try
                                                {
                                                    updateCommand.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("Integer Update: " + ex.Message);
                                                }
                                                con.Close();
                                            }
                                            if (TrendTags.FirstOrDefault(p => p.TagId == tag.Id && p.FieldDevice.IODeviceId == device.Id) != null)
                                            {
                                                InsertTrendValue(device, null, tag.Id, uintValue, time);
                                            }
                                            {
                                                //Insert
                                                InsertAnalogValue(device, null, tag.Id, uintValue, time);

                                                //check for alarms
                                                AlarmTag atag = AlarmTags.FirstOrDefault(p => p.FieldDevice.IODevice.Id == device.Id && p.TagId == tag.Id);
                                                if (atag != null)
                                                {
                                                    if (atag.LowSetPoint != null && uintValue < atag.LowSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id,1, "Low Set Point Alarm", (float)uintValue, time);
                                                    }
                                                    else if (atag.HighSetPoint != null && uintValue > atag.HighSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "High Set Point Alarm", (float)uintValue, time);
                                                    }
                                                    else
                                                    {
                                                        UpdateAlarm(atag.Id, 1, time);
                                                    }
                                                }
                                            }
                                            break;
                                        case DataType.DINT:
                                            var dintbytes = modbusClient.ReadHoldingRegisters<byte>(unitIdentifier, tag.Address, 4);
                                            Int32 dintValue = BitConverter.ToInt32(new byte[] { dintbytes[1], dintbytes[0], dintbytes[3], dintbytes[2] }, 0);
                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                            {
                                                con.Open();
                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + ") " +
                                                            "BEGIN UPDATE Live SET Time = @LogTime, Value = " + dintValue + " " +
                                                            "WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + "; " +
                                                            "END " +
                                                            "ELSE " +
                                                            "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                            "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime1, " + dintValue + ");" +
                                                            "END";

                                                string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                    "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + dintValue + ") " +
                                                    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + dintValue + ";";

                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                try
                                                {
                                                    updateCommand.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("Integer Update: " + ex.Message);
                                                }
                                                con.Close();
                                            }
                                            if (TrendTags.FirstOrDefault(p => p.TagId == tag.Id && p.FieldDevice.IODeviceId == device.Id) != null)
                                            {
                                                InsertTrendValue(device, null, tag.Id, dintValue, time);
                                            }
                                            {
                                                //Insert
                                                InsertAnalogValue(device, null, tag.Id, dintValue, time);

                                                //check for alarms
                                                AlarmTag atag = AlarmTags.FirstOrDefault(p => p.FieldDevice.IODevice.Id == device.Id && p.TagId == tag.Id);
                                                if (atag != null)
                                                {
                                                    if (atag.LowSetPoint != null && dintValue < atag.LowSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "Low Set Point Alarm", (float)dintValue, time);
                                                    }
                                                    else if (atag.HighSetPoint != null && dintValue > atag.HighSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "High Set Point Alarm", (float)dintValue, time);
                                                    }
                                                    else
                                                    {
                                                        UpdateAlarm(atag.Id, 1, time);
                                                    }
                                                }
                                            }
                                            break;
                                        case DataType.UDINT:
                                            var udintbytes = modbusClient.ReadHoldingRegisters<byte>(unitIdentifier, tag.Address, 4);
                                            UInt32 udintValue = BitConverter.ToUInt32(new byte[] { udintbytes[3], udintbytes[2], udintbytes[1], udintbytes[0] }, 0);
                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                            {
                                                con.Open();
                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + ") " +
                                                            "BEGIN UPDATE Live SET Time = @LogTime, Value = " + udintValue + " " +
                                                            "WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + "; " +
                                                            "END " +
                                                            "ELSE " +
                                                            "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                            "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime1, " + udintValue + ");" +
                                                            "END";

                                                string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                    "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + udintValue + ") " +
                                                    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + udintValue + ";";

                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                try
                                                {
                                                    updateCommand.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("Integer Update: " + ex.Message);
                                                }
                                                con.Close();
                                            }
                                            if (TrendTags.FirstOrDefault(p => p.TagId == tag.Id && p.FieldDevice.IODeviceId == device.Id) != null)
                                            {
                                                InsertTrendValue(device, null, tag.Id, udintValue, time);
                                            }
                                            {
                                                //Insert
                                                InsertAnalogValue(device, null, tag.Id, udintValue, time);

                                                //check for alarms
                                                AlarmTag atag = AlarmTags.FirstOrDefault(p => p.FieldDevice.IODevice.Id == device.Id && p.TagId == tag.Id);
                                                if (atag != null)
                                                {
                                                    if (atag.LowSetPoint != null && udintValue < atag.LowSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "Low Set Point Alarm", (float)udintValue, time);
                                                    }
                                                    else if (atag.HighSetPoint != null && udintValue > atag.HighSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "High Set Point Alarm", (float)udintValue, time);
                                                    }
                                                    else
                                                    {
                                                        UpdateAlarm(atag.Id, 1, time);
                                                    }
                                                }
                                            }
                                            break;
                                        case DataType.REAL:
                                            var realbytes = modbusClient.ReadHoldingRegisters<byte>(unitIdentifier, tag.Address, 4);
                                            Single realValue = BitConverter.ToSingle(new byte[] { realbytes[1], realbytes[0], realbytes[3], realbytes[2] }, 0);
                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                            {
                                                con.Open();
                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + ") " +
                                                            "BEGIN UPDATE Live SET Time = @LogTime, Value = " + realValue + " " +
                                                            "WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + "; " +
                                                            "END " +
                                                            "ELSE " +
                                                            "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                            "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime1, " + realValue + ");" +
                                                            "END";

                                                string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                    "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + realValue + ") " +
                                                    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + realValue + ";";

                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                try
                                                {
                                                    updateCommand.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("Integer Update: " + ex.Message);
                                                }
                                                con.Close();
                                            }
                                            if (TrendTags.FirstOrDefault(p => p.TagId == tag.Id && p.FieldDevice.IODeviceId == device.Id) != null)
                                            {
                                                InsertTrendValue(device, null, tag.Id, realValue, time);
                                            }
                                            {
                                                //Insert 
                                                InsertAnalogValue(device, null, tag.Id, realValue, time);

                                                //check for alarms
                                                AlarmTag atag = AlarmTags.FirstOrDefault(p => p.FieldDevice.IODevice.Id == device.Id && p.TagId == tag.Id);
                                                if (atag != null)
                                                {
                                                    if (atag.LowSetPoint != null && realValue < atag.LowSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "Low Set Point Alarm", (float)realValue, time);
                                                    }
                                                    else if (atag.HighSetPoint != null && realValue > atag.HighSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "High Set Point Alarm", (float)realValue, time);
                                                    }
                                                    else
                                                    {
                                                        UpdateAlarm(atag.Id, 1, time);
                                                    }
                                                }
                                            }
                                            break;

                                        case DataType.INT64:
                                            var longbytes = modbusClient.ReadHoldingRegisters<byte>(unitIdentifier, tag.Address, 8).ToArray();
                                            //Int64 int64Value = BitConverter.ToInt64(new byte[] { longbytes[1], longbytes[0], longbytes[3], longbytes[2], longbytes[5], longbytes[4], longbytes[7], longbytes[6] }, 0);
                                            Single real1 = BitConverter.ToSingle(new byte[] { longbytes[7], longbytes[6], longbytes[5], longbytes[4] });
                                            Single real2 = BitConverter.ToSingle(new byte[] { longbytes[1], longbytes[6], longbytes[3], longbytes[2] });
                                            double int64Value = real2 * 4294967.296 + (real1 / 1000);
                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                            {
                                                con.Open();
                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + ") " +
                                                    "BEGIN UPDATE Live SET Time = @LogTime, Value = " + int64Value + " " +
                                                    "WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + "; " +
                                                    "END " +
                                                    "ELSE " +
                                                    "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                    "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime1, " + int64Value + ");" +
                                                    "END";


                                                string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                    "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + int64Value + ") " +
                                                    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + int64Value + ";";

                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                try
                                                {
                                                    updateCommand.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("Gateway Update: " + ex.Message);
                                                }
                                                con.Close();
                                            }
                                            if (TrendTags.FirstOrDefault(p => p.TagId == tag.Id && p.FieldDevice.IODeviceId == device.Id) != null)
                                            {
                                                InsertTrendValue(device, null, tag.Id, int64Value, time);
                                            }
                                            {
                                                //Insert
                                                InsertAnalogValue(device, null, tag.Id, int64Value, time);

                                                //check for alarms
                                                AlarmTag atag = AlarmTags.FirstOrDefault(p => p.FieldDevice.IODevice.Id == device.Id && p.TagId == tag.Id);
                                                if (atag != null)
                                                {
                                                    if (atag.LowSetPoint != null && int64Value < atag.LowSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "Low Set Point Alarm", (float)int64Value, time);
                                                    }
                                                    else if (atag.HighSetPoint != null && int64Value > atag.HighSetPoint)
                                                    {
                                                        InsertAlarm(atag.Id, 1, "High Set Point Alarm", (float)int64Value, time);
                                                    }
                                                    else
                                                    {
                                                        UpdateAlarm(atag.Id, 1, time);
                                                    }
                                                }
                                            }
                                            break;
                                        case DataType.WBOOL:
                                            var wboolBytes = modbusClient.ReadHoldingRegisters<byte>(unitIdentifier, tag.Address, 2);
                                            var bitArray = new BitArray(new byte[] { wboolBytes[1], wboolBytes[0] });
                                            int wres = Convert.ToInt16(bitArray[tag.Bit]);
                                           
                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                            {
                                                con.Open();
                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + ") " +
                                                           "BEGIN UPDATE Live SET Time = @LogTime, Value = " + wres + " " +
                                                           "WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + "; " +
                                                           "END " +
                                                           "ELSE " +
                                                           "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                           "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + wres + ");" +
                                                           "END";

                                                string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                    "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + wres + ") " +
                                                    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + wres + ";";

                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                try
                                                {
                                                    updateCommand.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("Integer Update: " + ex.Message);
                                                }
                                                con.Close();
                                            }
                                            {
                                                try
                                                {
                                                    string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)}";
                                                    using (SqlConnection con = new SqlConnection(dconString))
                                                    {
                                                        con.Open();

                                                        int LogOnValue = 1;
                                                        bool isPermit = false;
                                                        bool isAlarm = false;
                                                        var alarmTag = AlarmTags.FirstOrDefault(x => x.TagId == tag.Id);

                                                        if (alarmTag != null)
                                                        {
                                                            isAlarm = true;
                                                            LogOnValue = (bool)alarmTag.LogOn ? 1 : 0;
                                                        }
                                                        else
                                                        {
                                                            var permit = Permits.FirstOrDefault(x => x.TagId == tag.Id);
                                                            
                                                            if (permit != null)
                                                            {
                                                                isPermit = true;
                                                                LogOnValue = 1;
                                                            }
                                                        }
                                                        SqlCommand pvalCmd = new SqlCommand("SELECT TOP 1 TagId FROM DigitalData WHERE TagID = " + tag.Id + " AND OffTime IS NULL", con);
                                                        SqlDataAdapter pvalAdap = new SqlDataAdapter(pvalCmd);
                                                        DataTable pvDt = new DataTable();
                                                        pvalAdap.Fill(pvDt);
                                                        if (pvDt.Rows.Count == 0)
                                                        {
                                                            if (wres == LogOnValue)
                                                            {
                                                                
                                                                if (isPermit)
                                                                {
                                                                    SqlCommand permitInsert = new SqlCommand(@"INSERT INTO PermitsData(PermitId,LogTime)VALUES(@PermitId,@LogTime)", con);
                                                                    permitInsert.Parameters.AddWithValue("@PermitId", tag.Id);
                                                                    permitInsert.Parameters.AddWithValue("@LogTime", time);
                                                                    permitInsert.ExecuteNonQuery();
                                                                }
                                                                if (isAlarm)
                                                                {
                                                                    InsertAlarm(tag.Id, 1, "Log On", wres, time);
                                                                }
                                                                SqlCommand insCommand = new SqlCommand("INSERT INTO DigitalData (TagID,OnTime) VALUES (@TagID,@TriggerTime)",con);
                                                                insCommand.Parameters.AddWithValue("@TagID", tag.Id);
                                                                insCommand.Parameters.AddWithValue("@TriggerTime", time);
                                                                insCommand.ExecuteNonQuery();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (wres != LogOnValue)
                                                            {
                                                                if (isPermit)
                                                                {
                                                                    SqlCommand updatePermitCmd = new SqlCommand(@"UPDATE PermitsData SET ResetTime = @ResetTime WHERE PermitId = @PermitId AND ResetTime IS NULL", con);
                                                                    updatePermitCmd.Parameters.AddWithValue("@PermitId", tag.Id);
                                                                    updatePermitCmd.Parameters.Add("@ResetTime", SqlDbType.DateTime).Value = time;
                                                                    updatePermitCmd.ExecuteNonQuery();
                                                                }
                                                                if (isAlarm)
                                                                {
                                                                    UpdateAlarm(tag.Id, 1, time);
                                                                }
                                                                SqlCommand updateCmd = new SqlCommand(@"UPDATE DigitalData SET OffTime = @OffTime WHERE TagID = " + tag.Id + " AND OffTime IS NULL", con);
                                                                 updateCmd.Parameters.Add("@OffTime", SqlDbType.DateTime).Value = time;
                                                                 updateCmd.ExecuteNonQuery();
                                                            }
                                                        }
                                                        con.Close();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("digital Insert: TagId: " + tag.Id + " Value - " + wres + " - " + ex.Message);
                                                }
                                            }
                                            break;
                                        case DataType.BOOL:
                                            var byteRes = modbusClient.ReadCoils(unitIdentifier, tag.Address, 1).ToArray()[0];
                                            bool boolRes = (byteRes & (1 << 0)) != 0;
                                            int res = Convert.ToInt32(boolRes);
                                            using (SqlConnection con = new SqlConnection(ConfigConString))
                                            {
                                                con.Open();
                                                string comnd = "IF EXISTS (SELECT 1 FROM Live WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + ") " +
                                                           "BEGIN UPDATE Live SET Time = @LogTime, Value = " + res + " " +
                                                           "WHERE DevId = " + device.Id + " and SerDevId = 0 and ParamId = " + tag.Id + "; " +
                                                           "END " +
                                                           "ELSE " +
                                                           "BEGIN INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                           "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + res + ");" +
                                                           "END";

                                                string cmd = "INSERT INTO Live (DevId, SerDevId, ParamId, Time, Value) " +
                                                    "VALUES (" + device.Id + ", 0 , " + tag.Id + ", @LogTime, " + res + ") " +
                                                    "ON DUPLICATE KEY UPDATE Time = @LogTime1 , Value = " + res + ";";

                                                SqlCommand updateCommand = new SqlCommand(comnd, con);
                                                updateCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
                                                updateCommand.Parameters.Add("@LogTime1", SqlDbType.DateTime).Value = time;
                                                try
                                                {
                                                    updateCommand.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("Integer Update: " + ex.Message);
                                                }
                                                con.Close();
                                            }
                                            {
                                                try
                                                {
                                                    string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)}";
                                                    using (SqlConnection con = new SqlConnection(dconString))
                                                    {
                                                        con.Open();
                                                        int LogOnValue = 1;
                                                        bool isPermit = false;
                                                        bool isAlarm = false;

                                                        var alarmTag = AlarmTags.FirstOrDefault(x => x.TagId == tag.Id);
                                                        if (alarmTag != null)
                                                        {
                                                            LogOnValue = (bool)alarmTag.LogOn ? 1 : 0;
                                                            isAlarm = true;
                                                        }
                                                        else
                                                        {
                                                            var permit = Permits.FirstOrDefault(x => x.TagId == tag.Id);
                                                            if (permit != null)
                                                            {
                                                                isPermit = true;
                                                                LogOnValue = 1;
                                                            }
                                                        }
                                                        SqlCommand pvalCmd = new SqlCommand("SELECT TOP 1 TagId FROM DigitalData WHERE TagID = " + tag.Id + " AND OffTime IS NULL", con);
                                                        SqlDataAdapter pvalAdap = new SqlDataAdapter(pvalCmd);
                                                        DataTable pvDt = new DataTable();
                                                        pvalAdap.Fill(pvDt);
                                                        if (pvDt.Rows.Count == 0)
                                                        {
                                                            if (res == LogOnValue)
                                                            {
                                                                 SqlCommand insCommand = new SqlCommand("INSERT INTO DigitalData (TagID,OnTime) VALUES (@TagID,@TriggerTime)", con);
                                                                 insCommand.Parameters.AddWithValue("@TagID", tag.Id);
                                                                 insCommand.Parameters.AddWithValue("@TriggerTime", time);
                                                                 insCommand.ExecuteNonQuery();

                                                                if (isPermit)
                                                                {
                                                                    SqlCommand permitInsert = new SqlCommand(@"INSERT INTO PermitsData(PermitId,LogTime,Acknowledged)VALUES(@PermitId,@LogTime, 0)", con);
                                                                    permitInsert.Parameters.AddWithValue("@PermitId", tag.Id);
                                                                    permitInsert.Parameters.AddWithValue("@LogTime", time);                                                                    
                                                                    permitInsert.ExecuteNonQuery();
                                                                }
                                                                if(isAlarm)
                                                                {
                                                                    InsertAlarm(tag.Id, 1, "Log On", res, time);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (res != LogOnValue)
                                                            {
                                                                SqlCommand updateCmd = new SqlCommand(@"UPDATE DigitalData SET OffTime = @OffTime WHERE TagID = " + tag.Id + " AND OffTime IS NULL", con);
                                                                updateCmd.Parameters.Add("@OffTime", SqlDbType.DateTime).Value = time;
                                                                updateCmd.ExecuteNonQuery();

                                                                if (isPermit)
                                                                {
                                                                    SqlCommand updatePermitCmd = new SqlCommand(@"UPDATE PermitsData SET ResetTime = @ResetTime WHERE PermitId = @PermitId AND ResetTime IS NULL", con);
                                                                    updatePermitCmd.Parameters.AddWithValue("@PermitId", tag.Id);
                                                                    updatePermitCmd.Parameters.Add("@ResetTime", SqlDbType.DateTime).Value = time;
                                                                    updatePermitCmd.ExecuteNonQuery();
                                                                }
                                                                if(isAlarm)
                                                                {
                                                                    UpdateAlarm(tag.Id, 1, time);
                                                                }
                                                            }
                                                        }
                                                        con.Close();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.WriteLog("digital Insert: TagId: " + tag.Id + " Value - " + res + " - " + ex.Message);
                                                }
                                            }
                                            break;
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                            modbusClient.Disconnect();
                        }
                    }
                    break;
            }
        }

        public void Dispose()
        {
            foreach (Timer timer in Timers)
            {
                if (timer != null)
                    timer.Dispose();
            }
            _monthlyDbTimer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // resolve scoped services inside a scope
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<iDbRepository>();
                AlarmTags = repo.GetAlarmTags().ToList();
                //foreach (AlarmTag tag in AlarmTags)
                //{
                //    Log.WriteLog(tag.Name+" High: "+tag.HighSetPoint+" Low: "+tag.LowSetPoint);
                //}
                AlarmParameters = repo.GetAlarmParameters().ToList();
                //foreach (AlarmParameter param in AlarmParameters)
                //{
                //    Log.WriteLog(param.Name+" High: "+param.HighSetPoint+" Low: "+param.LowSetPoint);
                //}

                //Categories = repo.GetCategories().ToList();
                FieldDevices = repo.GetFieldDevices().ToList();
                IODevices = repo.GetIODevices().ToList();
                SerialDevices = repo.GetSerialDevices().ToList();
                SerialDeviceDrivers = repo.GetSerialDeviceDrivers().ToList();
                SerialDeviceParameters = repo.GetSerialDeviceParameters().ToList();
                SerialDeviceReadBlocks = repo.GetSerialDeviceReadBlocks().ToList();
                SerialDeviceRegisters = repo.GetSerialDeviceRegisters().ToList();
                TrendParameters  =  repo.GetTrendParameters().ToList();
                TrendTags  = repo.GetTrendTags().ToList();
                Tags = repo.GetTags().ToList();
                Permits = repo.GetPermits().ToList();
            }

            // ensure current month DB exists immediately and schedule subsequent monthly creation
            EnsureMonthlyDatabaseExists(DateTime.Now);
            ScheduleMonthlyDatabaseCreation();
            Log.WriteLog("new ");
            if (IODevices.Count > 0)
            {
                foreach (IODevice device in IODevices)
                {
                    if (device != null)
                    {
                        try
                        {
                            int second = 60 - DateTime.Now.Second;
                            Timers.Add(new Timer(DoWork, device, second, 15000));
                            Log.WriteLog("DataLogging has started for " + device.Name);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog("OnStart-Exception: " + ex.Message);
                        }
                    }
                }
            }
            Timers.Add(new Timer(CheckAlarms, null, 0, 5000));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (Timer timer in Timers)
            {
                if (timer != null)
                    timer.Dispose();
            }
            _monthlyDbTimer?.Dispose();
            return Task.CompletedTask;
        }

        public void CheckAlarms(object state)
        {
            try
            {
                DateTime time = DateTime.Now;
                string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)}";
                using (SqlConnection connection = new SqlConnection(dconString))
                {
                    connection.Open();
                    SqlCommand sqlCommand = connection.CreateCommand();

                    sqlCommand.CommandText = "select top 1 AlarmId from AlarmsData where ResetTime is null AND Acknowledged = 0";
                    var resObj = sqlCommand.ExecuteScalar();
                    if (resObj != null)
                    {
                        if (!buzzer)
                        {
                            soundPlayer.PlayLooping();
                            buzzer = true;
                        }
                    }
                    else
                    {
                        soundPlayer.Stop();
                        buzzer = false;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("CheckAlarms: " + ex.Message);
            }
        }
    }
}

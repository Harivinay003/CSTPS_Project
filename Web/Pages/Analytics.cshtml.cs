using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Xml.Linq;
using VirtualEMS.DataServices;
using VirtualEMS.Library;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace VirtualEMS.Web.Pages
{
    public class AnalyticsModel : PageModel
    {
        public iDbRepository Repository { get; }
        public IConfiguration Configuration { get; }
        public IEnumerable<SerialDevice> SerialDevices { get; set; }
        public IEnumerable<FieldDevice> FieldDevices { get; set; }
        public List<AlarmParameter> AlarmParameters { get; set; } = new List<AlarmParameter>();
        public List<AlarmTag> AlarmTags { get; set; } = new List<AlarmTag>();
        [BindProperty(SupportsGet = true)]
        public int ParentId { get; set; }      
        public int ChartType { get; set; }
        public int SpanType { get; set; }       
        [BindProperty(SupportsGet = true)]
        public DateTime? SelectedDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public int FieldDeviceId { get; set; }
        public AnalyticsModel(iDbRepository repository, IConfiguration configuration)
        {
            Repository = repository;
            Configuration = configuration;
            SerialDevices = repository.GetSerialDevices();
            FieldDevices = repository.GetFieldDevices();

        }    
        private double GetInitialValue(DateTime fTime,DateTime eTime,int devId,int paramId)
        {
            string database = fTime.ToString("MMM-yyyy");
            string connString = Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";
            try
            {
                using SqlConnection conn = new(connString);
                conn.Open();
                string query = @"
            SELECT TOP 1 Value
            FROM AnalogData
            WHERE DevId = @DevId
            AND SerDevId = 0
            AND ParamId = @ParamId
            AND LogTime BETWEEN @StartTime AND @EndTime
            ORDER BY LogTime";

                using SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@DevId", devId);
                cmd.Parameters.AddWithValue("@ParamId", paramId);
                cmd.Parameters.AddWithValue("@StartTime", fTime);
                cmd.Parameters.AddWithValue("@EndTime", eTime);
                object value = cmd.ExecuteScalar();
                if (value != null)
                    return Convert.ToDouble(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return 0;
        }
        private double GetFinalValue(DateTime fTime,DateTime eTime,int devId,int paramId)
        {
            string database = eTime.ToString("MMM-yyyy");
            string connString =Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";
            try
            {
                using SqlConnection conn = new(connString);
                conn.Open();
                string query = @"
            SELECT TOP 1 Value
            FROM AnalogData
            WHERE DevId = @DevId
            AND SerDevId = 0
            AND ParamId = @ParamId
            AND LogTime BETWEEN @StartTime AND @EndTime
            ORDER BY LogTime DESC";

                using SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@DevId", devId);
                cmd.Parameters.AddWithValue("@ParamId", paramId);
                cmd.Parameters.AddWithValue("@StartTime", fTime);
                cmd.Parameters.AddWithValue("@EndTime", eTime);
                object value = cmd.ExecuteScalar();
                if (value != null)
                    return Convert.ToDouble(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return 0;
        }
        private double GetConsumption(DateTime fTime,DateTime eTime,int devId,int serDevId,int paramId)
        {
            double initial = GetInitialValue(fTime,eTime,devId,paramId);
            double final = GetFinalValue( fTime,eTime,devId,paramId);
            return Math.Round(final - initial,2);
        }
        public IActionResult OnGetFieldDeviceData(int fieldDeviceId, DateTime? start, DateTime? end)
        {
            DateTime fTime = start ?? DateTime.Now.Date;
            DateTime eTime = end ?? fTime.AddDays(1).AddSeconds(-1);
            var fieldDevice = Repository
                .GetFieldDevices()
                .FirstOrDefault(f => f.Id == fieldDeviceId);
            if (fieldDevice == null)
            {
                return new JsonResult(new { error = "Field Device not found" });
            }
            var tags = Repository.GetTags()
         .Where(t =>
         t.FieldDevice.Id == fieldDevice.Id &&
         t.Units != null &&
         t.Units.ToUpper() == "KWH")
         .ToList();
            var timestamps = new List<string>();
            var values = new List<double>();
            for (DateTime t = fTime; t < eTime; t = t.AddHours(1))
            {
                DateTime tEnd = t.AddHours(1).AddSeconds(-1);
                double total = 0;
                foreach (var tag in tags)
                {
                    total += GetConsumption(t,tEnd,fieldDevice.IODeviceId,0,tag.Id);
                }
                timestamps.Add(t.ToString("yyyy-MM-dd HH:mm"));
                values.Add(total);
            }
            return new JsonResult(new
            {
                type = "hourly",
                timestamps = timestamps,
                values = values,
                label = fieldDevice.Name
            });
        }
        private int GetAlarmCount(int fieldDeviceId, DateTime startTime, DateTime endTime)
        {
            int count = 0;
            string database = startTime.ToString("MMM-yyyy");
            string connString = Configuration.GetConnectionString("DataDBConnString") + "; Initial Catalog=" + database + ";";
            using SqlConnection conn = new(connString);
            conn.Open();
            string query = @"
        SELECT AlarmId, AlarmType
        FROM AlarmsData
         WHERE LogTime BETWEEN @StartTime AND @EndTime
         AND ResetTime is NULL";

            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@StartTime", startTime);
            cmd.Parameters.AddWithValue("@EndTime", endTime);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int alarmId = reader.GetInt32(0);
                int alarmType = reader.GetInt32(1);
                bool belongsToDevice = false;
                if (alarmType == 0)
                {
                    var param = Repository.GetAlarmParameters()
                        .FirstOrDefault(x => x.SerialDeviceId == alarmId && (int)x.Category == 0);
                    belongsToDevice = param != null;
                }
                else
                {
                    var tag = Repository.GetAlarmTags()
                        .FirstOrDefault(x => x.TagId == alarmId && (int)x.Category == 0 && x.FieldDeviceId == fieldDeviceId);
                    belongsToDevice = tag != null;
                }
                if (belongsToDevice)
                    count++;
            }
            return count;
        }
        private int GetTripCount(int fieldDeviceId, DateTime startTime, DateTime endTime)
        {
            int count = 0;
            string database = startTime.ToString("MMM-yyyy");
            string connString =Configuration.GetConnectionString("DataDBConnString") + "; Initial Catalog=" + database + ";";
            using SqlConnection conn = new(connString);
            conn.Open();
            string query = @"
        SELECT AlarmId, AlarmType
        FROM AlarmsData
        WHERE LogTime BETWEEN @StartTime AND @EndTime
        AND ResetTime is NULL";

            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@StartTime", startTime);
            cmd.Parameters.AddWithValue("@EndTime", endTime);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int alarmId = reader.GetInt32(0);
                int alarmType = reader.GetInt32(1);
                bool belongsToDevice = false;
                if (alarmType == 0)
                {
                    var param = Repository.GetAlarmParameters()
                        .FirstOrDefault(x => x.SerialDeviceId == alarmId && (int)x.Category == 2 );
                    belongsToDevice = param != null;
                }
                else
                {
                    var tag = Repository.GetAlarmTags()
                        .FirstOrDefault(x => x.TagId == alarmId && (int)x.Category == 2 && x.FieldDeviceId == fieldDeviceId);
                    belongsToDevice = tag != null;
                }
                if (belongsToDevice)
                    count++;
            }
            return count;
        }
        private double GetRunHrs(int fieldDeviceId, DateTime startTime, DateTime endTime)
        {
            var fieldDevice = Repository.GetFieldDevices()
                .FirstOrDefault(f => f.Id == fieldDeviceId);

            if (fieldDevice == null)
                return 0;

            var runHrTags = Repository.GetTags()
                .Where(t =>
                    t.FieldDeviceId == fieldDeviceId &&
                    (
                        (t.Units != null && t.Units.ToUpper() == "RUN_HRS") ||
                        t.Name.ToUpper().Contains("RUN_HRS")
                    ))
                .ToList();

            if (!runHrTags.Any())
                return 0;

            double totalRunHours = 0;
            foreach (var tag in runHrTags)
            {
                double startReading = GetInitialValue(startTime, endTime, fieldDevice.IODeviceId, tag.Id);
                double endReading = GetFinalValue(startTime, endTime, fieldDevice.IODeviceId, tag.Id);
                totalRunHours += Math.Max(endReading - startReading, 0);
            }

            return Math.Round(totalRunHours, 2);
        }
        private double GetCurrentRunHours(int tagId)
        {
            string database = DateTime.Now.ToString("MMM-yyyy");
            string connString = Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";
            using SqlConnection conn = new(connString);
            conn.Open();

            string query = @"
                  SELECT TOP 1 Value
                  FROM AnalogData
                  WHERE ParamId=@TagID
                  ORDER BY LogTime DESC";

            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@TagID", tagId);
            object result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value)
                return 0;

            return Convert.ToDouble(result);
        }
     
        public async Task<IActionResult> OnGetFieldDeviceDashboardAsync(int fieldDeviceId, string? selectedDate, string graphType = "current")
        {
            DashboardData result = new();
            DateTime startTime = string.IsNullOrEmpty(selectedDate)? DateTime.Today: DateTime.Parse(selectedDate);
            DateTime endTime;
            if (startTime.Date == DateTime.Today)
            {
                endTime = DateTime.Now;
            }
            else
            {
                endTime = startTime.Date.AddDays(1).AddSeconds(-1);
            }
            var fieldDevice = Repository
                .GetFieldDevices()
                .FirstOrDefault(f => f.Id == fieldDeviceId);

            var kwhTags = Repository.GetTags()
    .Where(t =>
        t.FieldDeviceId == fieldDeviceId &&
        t.Units != null &&
        t.Units.ToUpper() == "KWH")
    .ToList();

            result.EnergyConsumption = 0;
            foreach (var tag in kwhTags)
            {
                result.EnergyConsumption += GetConsumption(startTime, endTime, fieldDevice.IODeviceId, 0, tag.Id);
            }
            result.EnergyConsumption = Math.Round(result.EnergyConsumption, 2);
            result.RunHours = GetRunHrs(fieldDeviceId, startTime, endTime);
            result.CurrentRunHours = 0;

            var runHrTags = Repository.GetTags()
                .Where(t =>
                    t.FieldDeviceId == fieldDeviceId &&
                    (
                        (t.Units != null && t.Units.ToUpper() == "RUN_HRS") ||
                        t.Name.ToUpper().Contains("RUN_HRS")
                    ))
                .ToList();

            foreach (var tag in runHrTags)
            {
                result.CurrentRunHours += GetCurrentRunHours(tag.Id);
            }
            result.CurrentRunHours = Math.Round(result.CurrentRunHours, 2);
            result.AlarmCount = GetAlarmCount(fieldDeviceId, startTime, endTime);
            result.TripCount = GetTripCount(fieldDeviceId, startTime, endTime);          
            result.TrendTime = new List<string>();
            List<Tag> trendTags = graphType.ToLower() switch
            {
                "current" => Repository.GetTags()
                    .Where(t =>
                        t.FieldDeviceId == fieldDeviceId &&
                        t.Units != null &&
                        t.Units.ToUpper() == "AMPS")
                    .ToList(),

                "voltage" => Repository.GetTags()
                    .Where(t =>
                        t.FieldDeviceId == fieldDeviceId &&
                        t.Units != null &&
                        (t.Units.ToUpper() == "V" ||
                         t.Units.ToUpper() == "VOLT"))
                    .ToList(),

                "power" => Repository.GetTags()
                    .Where(t =>
                        t.FieldDeviceId == fieldDeviceId &&
                        t.Units != null &&
                        t.Units.ToUpper() == "KW")
                    .ToList(),

                _ => new List<Tag>()
            };     
            result.TrendTime = new List<string>();
            result.TrendSeries = new List<TrendSeries>();

            if (trendTags.Any())
            {
                var device = Repository.GetFieldDevices()
                    .First(f => f.Id == fieldDeviceId);                
                string database = startTime.ToString("MMM-yyyy");
                string connString = Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";                    
                DateTime graphEnd = startTime.Date == DateTime.Today ? DateTime.Now : startTime.Date.AddDays(1).AddSeconds(-1);
                DateTime graphStart = graphEnd.AddHours(-1);
                using SqlConnection conn = new(connString);
                conn.Open();

                bool firstSeries = true;
                foreach (var tag in trendTags)
                {
                    string sql = @"
                          SELECT LogTime,Value
                          FROM AnalogData
                          WHERE DevId=@DevId
                          AND SerDevId=0
                          AND ParamId=@ParamId
                          AND LogTime BETWEEN @StartTime AND @EndTime
                          ORDER BY LogTime";

                    using SqlCommand cmd = new(sql, conn);
                    cmd.Parameters.AddWithValue("@DevId", device.IODeviceId);
                    cmd.Parameters.AddWithValue("@ParamId", tag.Id);
                    cmd.Parameters.AddWithValue("@StartTime", graphStart);
                    cmd.Parameters.AddWithValue("@EndTime", graphEnd);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    TrendSeries series = new();
                    series.Name = tag.Name;
                    while (reader.Read())
                    {
                        if (firstSeries)
                        {
                            result.TrendTime.Add(Convert.ToDateTime(reader["LogTime"]).ToString("HH:mm"));
                        }
                        series.Values.Add(reader["Value"] == DBNull.Value ? 0 : Convert.ToDouble(reader["Value"]));
                    }
                    firstSeries = false;
                    result.TrendSeries.Add(series);
                }
            }
            switch (graphType.ToLower())
            {
                case "current":
                    result.GraphTitle = "Current Trends";
                    result.GraphUnit = "A";
                    break;
                case "voltage":
                    result.GraphTitle = "Voltage Trends";
                    result.GraphUnit = "V";
                    break;
                case "power":
                    result.GraphTitle = "Power Trends";
                    result.GraphUnit = "kW";
                    break;
            }
            DateTime date = string.IsNullOrWhiteSpace(selectedDate)? DateTime.Today: Convert.ToDateTime(selectedDate);
            var status = GetRunningStatus(fieldDeviceId, date);
            result.isRunning = status.IsRunning;
            if (status.Direction != "Running")
            {
                result.Direction = status.Direction;
            }
            else
            {
                result.Direction = "";
            }
            return new JsonResult(result);
        }
        public void OnGet()
        {
            FieldDevices = Repository.GetFieldDevices().OrderBy(f => f.Name).ToList();
            if (FieldDeviceId == 0 && FieldDevices.Any())
            {
                FieldDeviceId = FieldDevices.First().Id;
            }
            if (!SelectedDate.HasValue)
            {
                SelectedDate = DateTime.Today;
            }
            this.ChartType = ChartType;
            this.SpanType = SpanType;            
        }
        //runnning Status 
         private RunningStatus GetRunningStatus(int fieldDeviceId, DateTime selectedDate)
        {
            var runTags = Repository.GetTags()
                .Where(t =>
                    t.FieldDeviceId == fieldDeviceId &&
                    t.Units != null &&
                    t.Units.ToUpper() == "RUN_FB_ID")
                .ToList();

            if (!runTags.Any())
                return new RunningStatus
                {
                    IsRunning = false,
                    Direction = "Stopped"
                };

            DateTime start = selectedDate.Date;
            DateTime end = start.AddDays(1);
            string database = start.ToString("MMM-yyyy");
            string connString = Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";
            using SqlConnection conn = new(connString);
            conn.Open();

            foreach (var tag in runTags)
            {
                string sql = @"
                       SELECT TOP 1 OffTime
                       FROM DigitalData
                       WHERE TagID=@TagID
                       AND OnTime>=@Start
                       AND OnTime<@End
                       ORDER BY OnTime DESC";

                using SqlCommand cmd = new(sql, conn);

                cmd.Parameters.AddWithValue("@TagID", tag.Id);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);

                object offTime = cmd.ExecuteScalar();

                if (offTime == null)
                    continue;

                if (offTime == DBNull.Value)
                {
                    string text = (tag.Name + " " + tag.Description).ToUpper();

                    string direction = "Running";

                    if (text.Contains("MOT1") && text.Contains("FWD"))
                        direction = "(Motor 1- Forward)";
                    else if (text.Contains("MOT1") && text.Contains("REV"))
                        direction = "(Motor 1- Reverse)";
                    else if (text.Contains("MOT2") && text.Contains("FWD"))
                        direction = "(Motor 2- Forward)";
                    else if (text.Contains("MOT2") && text.Contains("REV"))
                        direction = "(Motor 2- Reverse)";
                    else if (text.Contains("A1") && text.Contains("7"))
                        direction = "(A1)";
                    else if (text.Contains("A2") && text.Contains("7"))
                        direction = "(A2)";
                    else if (text.Contains("B1") && text.Contains("7"))
                        direction = "(B1)";
                    else if (text.Contains("B2") && text.Contains("7"))
                        direction = "(B2)";
                    else if (text.Contains("FWD"))
                        direction = "(Forward)";
                    else if (text.Contains("REV"))
                        direction = "(Reverse)";

                    return new RunningStatus
                    {
                        IsRunning = true,
                        Direction = direction
                    };
                }
            }
            return new RunningStatus
            {
                IsRunning = false,
                Direction = "Stopped"
            };
        }
        //Run Hrs
        public IActionResult OnGetRunHours(int fieldDeviceId, DateTime? selectedDate)
        {
            // Digital RUN FB tag (for starts & logs)
            var runTags = Repository.GetTags()
                .Where(t =>
                    t.FieldDeviceId == fieldDeviceId &&
                    t.Units != null &&
                    t.Units.ToUpper() == "RUN_FB_ID");
            // All RUN_HRS tags
            var runHrTags = Repository.GetTags()
                .Where(t =>
                    t.FieldDeviceId == fieldDeviceId &&
                    (
                        (t.Units != null && t.Units.ToUpper() == "RUN_HRS") ||
                        t.Name.ToUpper().Contains("RUN_HRS")
                    ))
                .ToList();

            var fieldDevice = Repository.GetFieldDevices()
                .FirstOrDefault(f => f.Id == fieldDeviceId);

            if (!runTags.Any() || fieldDevice == null)
            {
                return new JsonResult(new
                {
                    todayRunHours = 0,
                    totalRunHours = 0,
                    startCount = 0,
                    isRunning = false,
                    logs = new List<object>()
                });
            }

            DateTime baseDate = selectedDate ?? DateTime.Today;
            DateTime startTime = baseDate.Date;
            DateTime endTime =  baseDate.Date == DateTime.Today ? DateTime.Now : baseDate.Date.AddDays(1).AddSeconds(-1);
            // Today's runtime
            double totalHours = 0;
            foreach (var tag in runHrTags)
            {
                double startReading = GetInitialValue(startTime, endTime, fieldDevice.IODeviceId, tag.Id);
                double endReading = GetFinalValue(startTime, endTime, fieldDevice.IODeviceId,tag.Id);
                totalHours += Math.Max(endReading - startReading, 0);
            }
            // Total runtime
            double currentRunHours = 0;
            foreach (var tag in runHrTags)
            {
                currentRunHours += GetCurrentRunHours(tag.Id);
            }
            string database = baseDate.ToString("MMM-yyyy");
            string connString = Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";           
            bool isRunning = false;
            List<RunLog> logs = new();
            using SqlConnection conn = new(connString);
            conn.Open();

            foreach (var runTag in runTags)
            {
                string query = @"
                       SELECT
                       OnTime,
                       OffTime
                       FROM DigitalData
                       WHERE TagID=@TagID
                       AND OnTime < @EndTime
                       AND (OffTime IS NULL OR OffTime > @StartTime)
                       ORDER BY OnTime";

                using SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@TagID", runTag.Id);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DateTime onTime = Convert.ToDateTime(reader["OnTime"]);
                    bool running = reader["OffTime"] == DBNull.Value;
                    DateTime? offTime = running? null: Convert.ToDateTime(reader["OffTime"]);
                    DateTime actualOn = onTime < startTime ? startTime : onTime;
                    DateTime actualOff = running? endTime: offTime.Value;
                    if (actualOff > endTime)
                        actualOff = endTime;

                    double hrs = Math.Max((actualOff - actualOn).TotalHours, 0);
                    string text = (runTag.Name + " " + runTag.Description).ToUpper();

                    string direction = "";

                    if (text.Contains("MOT1") && text.Contains("FWD"))
                        direction = "(Motor 1- Forward)";
                    else if (text.Contains("MOT1") && text.Contains("REV"))
                        direction = "(Motor 1- Reverse)";
                    else if (text.Contains("MOT2") && text.Contains("FWD"))
                        direction = "(Motor 2- Forward)";
                    else if (text.Contains("MOT2") && text.Contains("REV"))
                        direction = "(Motor 2- Reverse)";
                    else if (text.Contains("A1") && text.Contains("7"))
                        direction = "(A1)";
                    else if (text.Contains("A2") && text.Contains("7"))
                        direction = "(A2)";
                    else if (text.Contains("B1") && text.Contains("7"))
                        direction = "(B1)";
                    else if (text.Contains("B2") && text.Contains("7"))
                        direction = "(B2)";
                    else if(text.Contains("FWD"))
                        direction = "(Forward)";
                    else if (text.Contains("REV"))
                        direction = "(Reverse)";

                    if (running)
                        isRunning = true;

                    // Remove duplicates
                    bool exists = logs.Any(x =>
                        x.OnTime == onTime &&
                        x.OffTime == offTime );

                    if (!exists)
                    {
                        logs.Add(new RunLog
                        {
                            OnTime = onTime,
                            OffTime = offTime,
                            Runtime = Math.Round(hrs, 2),
                            IsRunning = running,
                            Direction = direction
                        });
                    }
                }
            }
            // Sort latest first
            logs = logs
                .OrderByDescending(x => x.OnTime)
                .ToList();

            // Starts are counted after removing duplicates
            int startCount = logs.Count;

            // Convert back to anonymous objects for JSON
            var logResult = logs.Select(x => new
            {
                onTime = x.OnTime,
                offTime = x.OffTime,
                runtime = x.Runtime,
                isRunning = x.IsRunning,
                direction = x.Direction
            });
            return new JsonResult(new
            {
                todayRunHours = Math.Round(totalHours, 2),
                totalRunHours = Math.Round(currentRunHours, 2),
                startCount,
                isRunning,
                logs = logResult
            });
        }
        //Energy Consupmtion
        private string GetInitial(DateTime fTime, DateTime eTime, int devId, int serDevId, int paramId)
        {
            string database = fTime.ToString("MMM-yyyy");
            string connString = Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";
            try
            {
                using SqlConnection conn = new(connString);
                conn.Open();
                string query = @"
            SELECT TOP 1 Value
            FROM AnalogData
            WHERE DevId = @DevId
            AND SerDevId = @SerDevId
            AND ParamId = @ParamId
            AND LogTime BETWEEN @StartTime AND @EndTime
            ORDER BY LogTime";

                using SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@DevId", devId);
                cmd.Parameters.AddWithValue("@SerDevId", serDevId);
                cmd.Parameters.AddWithValue("@ParamId", paramId);
                cmd.Parameters.AddWithValue("@StartTime", fTime);
                cmd.Parameters.AddWithValue("@EndTime", eTime);
                object value = cmd.ExecuteScalar();

                return value?.ToString() ?? "0";
            }
            catch
            {
                return "0";
            }
        }
        private string GetFinal(DateTime fTime, DateTime eTime, int devId, int serDevId, int paramId)
        {
            string database = eTime.ToString("MMM-yyyy");
            string connString = Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";
            try
            {
                using SqlConnection conn = new(connString);
                conn.Open();
                string query = @"
            SELECT TOP 1 Value
            FROM AnalogData
            WHERE DevId = @DevId
            AND SerDevId = @SerDevId
            AND ParamId = @ParamId
            AND LogTime BETWEEN @StartTime AND @EndTime
            ORDER BY LogTime DESC";

                using SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@DevId", devId);
                cmd.Parameters.AddWithValue("@SerDevId", serDevId);
                cmd.Parameters.AddWithValue("@ParamId", paramId);
                cmd.Parameters.AddWithValue("@StartTime", fTime);
                cmd.Parameters.AddWithValue("@EndTime", eTime);
                object value = cmd.ExecuteScalar();

                return value?.ToString() ?? "0";
            }
            catch
            {
                return "0";
            }
        }
        public double GetConsumptionForModal(DateTime fTime, DateTime eTime, int devId, int serDevId, int paramId)
        {
            double initial;
            double final;
            try
            {
                initial = Convert.ToDouble(GetInitial(fTime, eTime, devId, serDevId, paramId));
            }
            catch
            {
                return 0;
            }
            try
            {
                final = Convert.ToDouble(GetFinal(fTime, eTime, devId, serDevId, paramId));
            }
            catch
            {
                return 0;
            }
            return Math.Max(final - initial, 0);
        }
        public IActionResult OnGetEnergyData( string duration = "day", int fieldDeviceId = 0, DateTime? selectedDate = null)
        {
            if (fieldDeviceId == 0)
            {
                fieldDeviceId = Repository.GetFieldDevices()
                    .OrderBy(f => f.Name)
                    .FirstOrDefault()?.Id ?? 0;
            }

            var fieldDevice = Repository.GetFieldDevices()
                .FirstOrDefault(f => f.Id == fieldDeviceId);

            if (fieldDevice == null)
            {
                return new JsonResult(new
                {
                    xData = new List<string>(),
                    series = new List<EnergySeries>()
                });
            }

            var tags = Repository.GetTags()
                .Where(t =>
                    t.FieldDeviceId == fieldDeviceId &&
                    t.Units != null &&
                    t.Units.ToUpper() == "KWH")
                .ToList();

            if (!tags.Any())
            {
                return new JsonResult(new
                {
                    xData = new List<string>(),
                    series = new List<EnergySeries>()
                });
            }

            List<string> xData = new();
            List<EnergySeries> series = new();
            DateTime now = selectedDate ?? DateTime.Today;

            if (now.Date == DateTime.Today)
                now = DateTime.Now;
            else
                now = now.Date.AddDays(1).AddSeconds(-1);

            foreach (var tag in tags)
            {
                EnergySeries s = new();
                s.Name = tag.Name;

                if (duration == "shift")
                {
                    DateTime shiftStart = now.Hour < 14 ? now.Date.AddHours(6): now.Hour < 22 ? now.Date.AddHours(14): now.Date.AddHours(22);
                    bool first = xData.Count == 0;
                    for (DateTime t = shiftStart; t < now; t = t.AddHours(1))
                    {
                        double consumption = GetConsumptionForModal(t, t.AddHours(1), fieldDevice.IODeviceId, 0,tag.Id);
                        s.Values.Add(Math.Round(consumption, 2));
                        if (first)
                            xData.Add(t.ToString("hh tt"));
                    }
                }
                else if (duration == "day")
                {
                    DateTime dayStart = now.Date;
                    int totalHours = now.Date == DateTime.Today? now.Hour + 1: 24;
                    bool first = xData.Count == 0;

                    for (int h = 0; h < totalHours; h++)
                    {
                        DateTime from = dayStart.AddHours(h);
                        DateTime to = from.AddHours(1);
                        double consumption = GetConsumptionForModal(from, to, fieldDevice.IODeviceId, 0, tag.Id);
                        s.Values.Add(Math.Round(consumption, 2));

                        if (first)
                            xData.Add(from.ToString("h tt"));
                    }
                }
                else if (duration == "week")
                {
                    DateTime weekStart = now.Date.AddDays(-6);
                    bool first = xData.Count == 0;

                    for (int i = 0; i < 7; i++)
                    {
                        DateTime from = weekStart.AddDays(i);
                        DateTime to = from.AddDays(1);
                        double consumption = GetConsumptionForModal(from,to, fieldDevice.IODeviceId, 0,tag.Id);
                        s.Values.Add(Math.Round(consumption, 2));
                        if (first)
                            xData.Add(from.ToString("ddd"));
                    }
                }
                else
                {
                    DateTime monthStart = new DateTime(now.Year, now.Month, 1);
                    bool first = xData.Count == 0;
                    while (monthStart < now)
                    {
                        DateTime weekEnd = monthStart.AddDays(7);
                        if (weekEnd > now)
                            weekEnd = now;

                        double consumption = GetConsumptionForModal( monthStart, weekEnd, fieldDevice.IODeviceId, 0,tag.Id);
                        s.Values.Add(Math.Round(consumption, 2));
                        if (first)
                            xData.Add("Week " + (xData.Count + 1));
                        monthStart = weekEnd;
                    }
                }
                series.Add(s);
            }
            return new JsonResult(new
            {
                xData,
                series
            });
        }
        //Alarms 
        public IActionResult OnGetAlarmDetails(string duration = "day", int fieldDeviceId = 0, DateTime? selectedDate = null)
        {
            AlarmTags = Repository.GetAlarmTags().ToList();
            AlarmParameters = Repository.GetAlarmParameters().ToList();
            var alarms = new List<object>();
            DateTime baseDate = selectedDate ?? DateTime.Today;
            DateTime startTime;
            DateTime endTime;
            switch (duration.ToLower())
            {
                case "shift":
                    if (baseDate.Date == DateTime.Today)
                    {
                        DateTime now = DateTime.Now;
                        if (now.Hour < 14)
                        {
                            startTime = now.Date.AddHours(6);
                            endTime = now.Date.AddHours(14);
                        }
                        else if (now.Hour < 22)
                        {
                            startTime = now.Date.AddHours(14);
                            endTime = now.Date.AddHours(22);
                        }
                        else
                        {
                            startTime = now.Date.AddHours(22);
                            endTime = now.Date.AddDays(1).AddHours(6);
                        }
                    }
                    else
                    {
                        startTime = baseDate.Date;
                        endTime = baseDate.Date.AddDays(1);
                    }
                    break;

                case "day":
                    startTime = baseDate.Date;
                    endTime = baseDate.Date.AddDays(1);
                    break;
                case "week":
                    startTime = baseDate.Date.AddDays(-6);
                    endTime = baseDate.Date.AddDays(1);
                    break;
                case "month":
                    startTime = new DateTime(baseDate.Year, baseDate.Month, 1);
                    endTime = startTime.AddMonths(1).AddSeconds(-1);
                    break;
                default:
                    startTime = baseDate.Date;
                    endTime = baseDate.Date.AddDays(1);
                    break;
            }
            string database = startTime.ToString("MMM-yyyy");
            string connString = Configuration.GetConnectionString("DataDBConnString") + "; Initial Catalog=" + database + ";";
            try
            {
                using SqlConnection conn = new(connString);
                conn.Open();
                string query = @"
                SELECT
                AlarmId,
                AlarmType,
                LogTime,
                Comment,
                ResetTime
            FROM AlarmsData
            WHERE LogTime BETWEEN @StartTime AND @EndTime
            ORDER BY LogTime DESC";

                using SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int alarmId = reader.GetInt32(0);
                    int alarmType = reader.GetInt32(1);
                    string tagName = "";
                    if (alarmType == 0)
                    {
                        var param = AlarmParameters.FirstOrDefault(x => x.SerialDeviceId == alarmId && x.Category == EventCategory.Alarm);
                        if (param == null)
                            continue;
                        tagName = param?.Name ?? "";
                    }
                    else
                    {
                        var tag = AlarmTags.FirstOrDefault(x => x.TagId == alarmId && x.Category == EventCategory.Alarm && x.FieldDeviceId == fieldDeviceId);
                        if (tag == null)
                            continue;
                        tagName = tag?.Name ?? "";
                    }

                    DateTime logTime = reader.GetDateTime(2);
                    string comment = reader["Comment"]?.ToString() ?? "";
                    DateTime? resetTime = reader["ResetTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["ResetTime"]);
                    alarms.Add(new
                    {
                        tagName,
                        alarmDescription = comment,
                        alarmOnTime = logTime,
                        alarmOffTime = resetTime,
                        status = resetTime == null ? "Active" : "Cleared"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    error = ex.Message
                });
            }
            return new JsonResult(alarms);
        }
        //Trips 
        public IActionResult OnGetTripDetails(string duration = "day", int fieldDeviceId = 0, DateTime? selectedDate = null)
        {
            AlarmTags = Repository.GetAlarmTags().ToList();
            AlarmParameters = Repository.GetAlarmParameters().ToList();
            if (fieldDeviceId == 0)
            {
                fieldDeviceId = Repository.GetFieldDevices()
                    .OrderBy(f => f.Name)
                    .FirstOrDefault()?.Id ?? 0;
            }

            var trips = new List<object>();
            DateTime baseDate = selectedDate ?? DateTime.Today;
            DateTime startTime;
            DateTime endTime;
            switch (duration.ToLower())
            {
                case "shift":
                    if (baseDate.Date == DateTime.Today)
                    {
                        DateTime now = DateTime.Now;
                        if (now.Hour < 14)
                        {
                            startTime = now.Date.AddHours(6);
                            endTime = now.Date.AddHours(14);
                        }
                        else if (now.Hour < 22)
                        {
                            startTime = now.Date.AddHours(14);
                            endTime = now.Date.AddHours(22);
                        }
                        else
                        {
                            startTime = now.Date.AddHours(22);
                            endTime = now.Date.AddDays(1).AddHours(6);
                        }
                    }
                    else
                    {
                        startTime = baseDate.Date;
                        endTime = baseDate.Date.AddDays(1);
                    }
                    break;
                case "day":
                    startTime = baseDate.Date;
                    endTime = baseDate.Date.AddDays(1);
                    break;
                case "week":
                    startTime = baseDate.Date.AddDays(-6);
                    endTime = baseDate.Date.AddDays(1);
                    break;
                case "month":
                    startTime = new DateTime(baseDate.Year, baseDate.Month, 1);
                    endTime = startTime.AddMonths(1);
                    break;
                default:
                    startTime = baseDate.Date;
                    endTime = baseDate.Date.AddDays(1);
                    break;
            }
            string database = baseDate.ToString("MMM-yyyy");
            string connString = Configuration.GetConnectionString("DataDBConnString") + "; Initial Catalog=" + database + ";";
            try
            {
                using SqlConnection conn = new(connString);
                conn.Open();
                string query = @"
                SELECT
                AlarmId,
                AlarmType,
                LogTime,
                Comment,
                ResetTime
            FROM AlarmsData
            WHERE LogTime BETWEEN @StartTime AND @EndTime
            ORDER BY LogTime DESC";

                using SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int alarmId = reader.GetInt32(0);
                    int alarmType = reader.GetInt32(1);
                    string tagName = "";
                    if (alarmType == 0)
                    {
                        var param = AlarmParameters.FirstOrDefault(x => x.SerialDeviceId == alarmId && x.Category == EventCategory.Trip);

                        if (param == null)
                            continue;
                        tagName = param.Name;
                    }
                    else
                    {
                        var tag = AlarmTags.FirstOrDefault(x => x.TagId == alarmId && x.Category == EventCategory.Trip);

                        if (tag == null)
                            continue;
                        if (fieldDeviceId > 0 && tag.FieldDeviceId != fieldDeviceId)
                        {
                            continue;
                        }
                        tagName = tag.Name;
                    }
                    DateTime tripTime = reader.GetDateTime(2);
                    string comment = reader["Comment"]?.ToString() ?? "";
                    DateTime? resetTime = reader["ResetTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["ResetTime"]);
                    trips.Add(new
                    {
                        tagName,
                        tripDescription = comment,
                        tripTime,
                        resetTime,
                        status = resetTime == null ? "Active" : "Reset"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
            return new JsonResult(trips);
        }
        //graph 
        public IActionResult OnGetTrend(int fieldDeviceId, string graphType = "current", DateTime? from = null, DateTime? to = null)
        {
            DateTime startTime = from ?? DateTime.Now.AddHours(-24);
            DateTime endTime = to ?? DateTime.Now;

            var fieldDevice = Repository.GetFieldDevices()
                .FirstOrDefault(f => f.Id == fieldDeviceId);

            if (fieldDevice == null)
            {
                return new JsonResult(new
                {
                    trendTime = new List<string>(),
                    trendSeries = new List<TrendSeries>()
                });
            }

            List<Tag> trendTags = graphType.ToLower() switch
            {
                "current" => Repository.GetTags()
                    .Where(t =>
                        t.FieldDeviceId == fieldDeviceId &&
                        t.Units != null &&
                        t.Units.ToUpper() == "AMPS")
                    .ToList(),

                "voltage" => Repository.GetTags()
                    .Where(t =>
                        t.FieldDeviceId == fieldDeviceId &&
                        t.Units != null &&
                        (t.Units.ToUpper() == "V" ||
                         t.Units.ToUpper() == "VOLT"))
                    .ToList(),

                "power" => Repository.GetTags()
                    .Where(t =>
                        t.FieldDeviceId == fieldDeviceId &&
                        t.Units != null &&
                        t.Units.ToUpper() == "KW")
                    .ToList(),

                _ => new List<Tag>()
            };

            List<string> trendTime = new();
            List<TrendSeries> trendSeries = new();

            if (trendTags.Any())
            {
                string database = startTime.ToString("MMM-yyyy");
                string connString = Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";
                using SqlConnection conn = new(connString);
                conn.Open();
                bool firstSeries = true;
                foreach (var tag in trendTags)
                {
                    string sql = @"
 SELECT
    LogTime,
    Value
FROM AnalogData
WHERE DevId=@DevId
AND SerDevId=0
AND ParamId=@ParamId
AND LogTime BETWEEN @StartTime AND @EndTime
ORDER BY LogTime";

                    using SqlCommand cmd = new(sql, conn);

                    cmd.Parameters.AddWithValue("@DevId", fieldDevice.IODeviceId);
                    cmd.Parameters.AddWithValue("@ParamId", tag.Id);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    using SqlDataReader reader = cmd.ExecuteReader();

                    TrendSeries series = new();

                    series.Name = tag.Name;

                    while (reader.Read())
                    {
                        if (firstSeries)
                        {
                            trendTime.Add(Convert.ToDateTime(reader["LogTime"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                        }
                        series.Values.Add(reader["Value"] == DBNull.Value ? 0: Convert.ToDouble(reader["Value"]));
                    }
                    firstSeries = false;
                    trendSeries.Add(series);
                }
            }
            return new JsonResult(new
            {
                trendTime,
                trendSeries,
                graphTitle = graphType.ToLower() switch
                {
                    "current" => "Current Trend",
                    "voltage" => "Voltage Trend",
                    "power" => "Power Trend",
                    _ => "Trend"
                },
                graphUnit = graphType.ToLower() switch
                {
                    "current" => "A",
                    "voltage" => "V",
                    "power" => "kW",
                    _ => ""
                }
            });
        }
    }
    public class DashboardData
    {
        public double RunHours { get; set; }
        public double CurrentRunHours { get; set; }
        public double EnergyConsumption { get; set; }
        public int AlarmCount { get; set; }
        public int TripCount { get; set; }
        public int PermitCount { get; set; }
        public List<string>? TrendTime { get; set; }
        public List<TrendSeries>? TrendSeries { get; set; }
        public string? GraphTitle { get; set; }
        public string? GraphUnit { get; set; }
        public List<PermitInfo>? Permits { get; internal set; }
        public bool isRunning { get; set; }
        public string Direction { get; set; } = "";
    }
    public class EnergySeries
    {
        public string Name { get; set; } = "";
        public List<double> Values { get; set; } = new();
    }
    public class TrendSeries
    {
        public string Name { get; set; } = "";
        public List<double> Values { get; set; } = new();
    }
    public class RunLog
    {
        public DateTime OnTime { get; set; }
        public DateTime? OffTime { get; set; }
        public double Runtime { get; set; }
        public bool IsRunning { get; set; }
         public string Direction { get; set; } = "";
    }
    public class RunningStatus
    {
        public bool IsRunning { get; set; }
        public string Direction { get; set; } = "Stopped";
    }
    public class PermitInfo
    {
        public string? PermitName { get; set; }
        public string? Status { get; set; }
    }
}

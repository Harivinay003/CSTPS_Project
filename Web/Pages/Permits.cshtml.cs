using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace VirtualEMS.Web.Pages
{
    public class PermitsModel : PageModel
    {
        public iDbRepository Repository { get; }
        public IConfiguration Configuration { get; }
        public IEnumerable<FieldDevice> FieldDevices { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? SelectedDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public int FieldDeviceId { get; set; }
        public PermitsModel(iDbRepository repository, IConfiguration configuration)
        {
            Repository = repository;
            Configuration = configuration;
            FieldDevices = repository.GetFieldDevices();
        }
        public void OnGet()
        {
            FieldDevices = Repository.GetFieldDevices()
                                     .OrderBy(f => f.Name)
                                     .ToList();
            if (FieldDeviceId == 0 && FieldDevices.Any())
            {
                FieldDeviceId = FieldDevices.First().Id;
            }
            if (!SelectedDate.HasValue)
            {
                SelectedDate = DateTime.Today;
            }
        }
        public IActionResult OnGetPermitDashboard()
        {
            var dashboard = new PermitDashboard();
            var devices = Repository.GetFieldDevices()
                                    .OrderBy(x => x.Name)
                                    .ToList();
            var logs = LoadPermitLogs(DateTime.Now);
            dashboard.TotalConveyors = devices.Count;
            var activeLogs = logs
                .Where(x => x.IsActive)
                .ToList();

            dashboard.ActivePermits = activeLogs.Count;
            var occupiedConveyors = activeLogs
                .Select(x => x.FieldDeviceId)
                .Distinct()
                .ToHashSet();

            dashboard.OccupiedConveyors = occupiedConveyors.Count;
            dashboard.ClearConveyors = dashboard.TotalConveyors -  dashboard.OccupiedConveyors;
            dashboard.FleetAvailability = dashboard.TotalConveyors == 0 ? 0: Math.Round( dashboard.ClearConveyors * 100.0 /  dashboard.TotalConveyors, 1);

            DateTime todayStart = DateTime.Today;
            DateTime tomorrowStart = todayStart.AddDays(1);

            double todayHours = 0;
            foreach (var log in logs)
            {
                DateTime start = log.LogTime > todayStart? log.LogTime: todayStart;
                DateTime end = (log.ResetTime ?? DateTime.Now) < tomorrowStart ? (log.ResetTime ?? DateTime.Now) : tomorrowStart;
                if (end > start)
                {
                    todayHours += (end - start).TotalHours;
                }
            }
            dashboard.PermitHoursToday = Math.Round(todayHours, 2);
            dashboard.TypeStatistics.Electrical = activeLogs.Count(x => x.Type == PermitType.Electrical);
            dashboard.TypeStatistics.Mechanical1 = activeLogs.Count(x => x.Type == PermitType.Mechanical1);
            dashboard.TypeStatistics.Mechanical2 = activeLogs.Count(x => x.Type == PermitType.Mechanical2);
            dashboard.TypeStatistics.Operation = activeLogs.Count(x => x.Type == PermitType.Operation);

            var typeCounts = activeLogs
                .GroupBy(x => x.Type)
                .Select(x => new
                {
                    Type = x.Key,
                    Count = x.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            if (typeCounts != null)
            {
                dashboard.HighestPermitType = GetPermitTypeName(typeCounts.Type);               
                dashboard.HighestPermitCount = typeCounts.Count;
            }
            var longest = activeLogs
                .OrderByDescending(x => x.DurationHours)
                .FirstOrDefault();
            if (longest != null)
            {
                dashboard.LongestOpenHours = Math.Round(longest.DurationHours, 2);
                dashboard.LongestOpenConveyor = longest.ConveyorName;
                dashboard.LongestOpenType = GetPermitTypeName(longest.Type);
            }
            foreach (var device in devices)
            {
                var deviceLogs = activeLogs
                    .Where(x => x.FieldDeviceId == device.Id)
                    .ToList();
                ConveyorPermitStatus status = new()
                {
                    FieldDeviceId = device.Id,
                    Name = device.Name?? "",
                    IsPermitActive = deviceLogs.Any(),
                    MainPermit = deviceLogs.Any( x => x.Type == PermitType.Permit),
                    Electrical = deviceLogs.Any(x => x.Type == PermitType.Electrical),
                    Mechanical1 = deviceLogs.Any(x => x.Type == PermitType.Mechanical1),
                    Mechanical2 = deviceLogs.Any(x => x.Type == PermitType.Mechanical2),
                    Operation = deviceLogs.Any(x => x.Type == PermitType.Operation)
                };
                status.DominantPermit = GetDominantPermit(status);
                dashboard.Conveyors.Add(status);
            }
            return new JsonResult(dashboard);
        }
        public IActionResult OnGetPermitHistory(int fieldDeviceId,DateTime? fromDate,DateTime? toDate,PermitType? permitType = null)
        {
            DateTime from = fromDate?.Date ?? DateTime.Today;
            DateTime to = toDate?.Date ?? DateTime.Today;
            DateTime end = to.AddDays(1);
            var logs = LoadPermitLogs(from, to);
            List<PermitHistory> history = new();
            var selectedLogs = logs
                .Where(x => x.FieldDeviceId == fieldDeviceId)
                .Where(x =>
                    x.IsActive ||
                    (x.LogTime >= from && x.LogTime < end));

            if (permitType.HasValue)
            {
                selectedLogs = selectedLogs
                    .Where(x => x.Type == permitType.Value);
            }
            foreach (var log in selectedLogs.OrderByDescending(x => x.LogTime))
            {
                TimeSpan duration = (log.ResetTime ?? DateTime.Now) - log.LogTime;
                history.Add(new PermitHistory
                {
                    PermitId = log.PermitTagId,
                    ConveyorName = log.ConveyorName,
                    PermitType = log.Type,
                    PermitName = log.PermitName,
                    WorkDescription = log.Comment,
                    IssuedTime = log.LogTime,
                    ClearedTime = log.ResetTime,
                    Duration = $"{(int)duration.TotalHours}h {duration.Minutes}m",
                    IsActive = log.IsActive
                });
            }
            return new JsonResult(history);
        }
        public IActionResult OnGetPermitTrend()
        {
            List<PermitTrend> trend = new();
            DateTime startDate = DateTime.Today.AddDays(-6);
            DateTime endDate = DateTime.Today;
            var logs = LoadPermitLogs(startDate, endDate);

            for (int i = 0; i < 7; i++)
            {
                DateTime day = startDate.AddDays(i);
                DateTime nextDay = day.AddDays(1);
                PermitTrend item = new()
                {
                    Day = day.ToString("ddd")
                };
                foreach (var log in logs)
                {
                    DateTime start = log.LogTime > day? log.LogTime: day;
                    DateTime end = (log.ResetTime ?? DateTime.Now) < nextDay ? (log.ResetTime ?? DateTime.Now): nextDay;
                    if (end <= start)
                        continue;
                    double hrs = (end - start).TotalHours;
                    switch (log.Type)
                    {
                        case PermitType.Electrical:
                            item.Electrical += hrs;
                            break;
                        case PermitType.Mechanical1:
                            item.Mechanical1 += hrs;
                            break;
                        case PermitType.Mechanical2:
                            item.Mechanical2 += hrs;
                            break;
                        case PermitType.Operation:
                            item.Operation += hrs;
                            break;
                    }
                }
                item.Electrical = Math.Round(item.Electrical, 2);
                item.Mechanical1 = Math.Round(item.Mechanical1, 2);
                item.Mechanical2 = Math.Round(item.Mechanical2, 2);
                item.Operation = Math.Round(item.Operation, 2);
                trend.Add(item);
            }
            return new JsonResult(trend);
        }
        public IActionResult OnGetTopConveyors()
        {
            DateTime from = DateTime.Today.AddDays(-6);
            DateTime to = DateTime.Today.AddDays(1);
            var logs = LoadPermitLogs(from, DateTime.Today);
            List<TopPermitConveyor> result = new();
            var grouped = logs
                .GroupBy(x => new
                {
                    x.FieldDeviceId,
                    x.ConveyorName
                });
            foreach (var group in grouped)
            {
                TopPermitConveyor item = new()
                {
                    Name = group.Key.ConveyorName
                };
                foreach (var log in group)
                {
                    DateTime start = log.LogTime > from ? log.LogTime: from;
                    DateTime logEnd = log.ResetTime ?? DateTime.Now;
                    DateTime end = logEnd < to ? logEnd : to;
                    if (end <= start)
                        continue;
                    double hours = (end - start).TotalHours;
                    switch (log.Type)
                    {
                        case PermitType.Electrical:
                            item.ElectricalHours += hours;
                            break;
                        case PermitType.Mechanical1:
                            item.Mechanical1Hours += hours;
                            break;
                        case PermitType.Mechanical2:
                            item.Mechanical2Hours += hours;
                            break;
                        case PermitType.Operation:
                            item.OperationHours += hours;
                            break;
                    }
                }
                item.ElectricalHours = Math.Round(item.ElectricalHours, 2);
                item.Mechanical1Hours = Math.Round(item.Mechanical1Hours, 2);
                item.Mechanical2Hours = Math.Round(item.Mechanical2Hours, 2);
                item.OperationHours = Math.Round(item.OperationHours, 2);

                item.TotalHours = Math.Round(
                    item.ElectricalHours +
                    item.Mechanical1Hours +
                    item.Mechanical2Hours +
                    item.OperationHours,
                    2);
                result.Add(item);
            }
            result = result
                .OrderByDescending(x => x.TotalHours)
                .Take(5)
                .ToList();
            return new JsonResult(result);
        }
        private List<PermitLog> LoadPermitLogs(DateTime from, DateTime to)
        {
            List<PermitLog> logs = new();
            DateTime month = new DateTime(from.Year, from.Month, 1);
            while (month <= to)
            {
                logs.AddRange(LoadPermitLogs(month));
                month = month.AddMonths(1);
            }
            return logs;
        }
        private List<PermitLog> LoadPermitLogs(DateTime date)
        {
            List<PermitLog> logs = new();

            var devices = Repository.GetFieldDevices().ToList();
            var permits = Repository.GetPermits().ToList();
            var tags = Repository.GetTags().ToList();
            // Assign Permit Type once
            foreach (var permit in permits)
            {
                var tag = tags.FirstOrDefault(x => x.Id == permit.TagId);
                if (tag != null)
                    permit.Type = GetPermitType(tag.Name?? "");
            }
            using SqlConnection conn = new(GetConnectionString(date));
            conn.Open();

            string sql = @"
                   SELECT
                   PermitId,
                   LogTime,
                   ResetTime,
                   Comment
                   FROM PermitsData";

            using SqlCommand cmd = new(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int permitTagId = Convert.ToInt32(reader["PermitId"]);
                var permit = permits.FirstOrDefault(x => x.TagId == permitTagId);
                if (permit == null)
                    continue;

                var device = devices.FirstOrDefault(x => x.Id == permit.FieldDeviceId);
                var tag = tags.FirstOrDefault(x => x.Id == permit.TagId);
                logs.Add(new PermitLog
                {
                    PermitTagId = permitTagId,
                    FieldDeviceId = permit.FieldDeviceId,
                    ConveyorName = device?.Name ?? "",
                    PermitName = tag?.Name ?? "",
                    Type = permit.Type,
                    LogTime = Convert.ToDateTime(reader["LogTime"]),
                    ResetTime = reader["ResetTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["ResetTime"]),
                    Comment = reader["Comment"]?.ToString() ?? ""
                });
            }
            return logs;
        }
        private string GetConnectionString(DateTime date)
        {
            string database = date.ToString("MMM-yyyy");
            return Configuration.GetConnectionString("DataDBConnString") + ";Initial Catalog=" + database + ";";
        }
        private string GetPermitTypeName(PermitType type)
        {
            return type switch
            {
                PermitType.Electrical => "Electrical",
                PermitType.Mechanical1 => "Mechanical-01",
                PermitType.Mechanical2 => "Mechanical-02",
                PermitType.Operation => "Operation",
                _ => "Permit"
            };
        }
        private PermitType GetDominantPermit(ConveyorPermitStatus status)
        {
            if (status.Electrical)
                return PermitType.Electrical;
            if (status.Mechanical1)
                return PermitType.Mechanical1;
            if (status.Mechanical2)
                return PermitType.Mechanical2;
            if (status.Operation)
                return PermitType.Operation;
            if (status.MainPermit)
                return PermitType.Permit;
            return PermitType.Permit;
        }
        private PermitType GetPermitType(string tagName)
        {
            tagName = tagName.ToUpper();
            if (tagName.EndsWith("_E"))
                return PermitType.Electrical;
            if (tagName.EndsWith("_M1"))
                return PermitType.Mechanical1;
            if (tagName.EndsWith("_M2"))
                return PermitType.Mechanical2;
            if (tagName.EndsWith("_OP"))
                return PermitType.Operation;
            return PermitType.Permit;
        }
    }
  public class PermitDashboard
    {
        public int TotalConveyors { get; set; }
        public int ClearConveyors { get; set; }
        public int OccupiedConveyors { get; set; }
        public double FleetAvailability { get; set; }
        public int ActivePermits { get; set; }
        public double PermitHoursToday { get; set; }
        public string HighestPermitType { get; set; } = "";
        public string LongestOpenType { get; set; } = "";
        public int HighestPermitCount { get; set; }
        public double LongestOpenHours { get; set; }
        public string LongestOpenConveyor { get; set; } = "";
        public PermitTypeStatistics TypeStatistics { get; set; } = new();
        public List<ConveyorPermitStatus> Conveyors { get; set; } = new();
    }
    public class ConveyorPermitStatus
    {
        public int FieldDeviceId { get; set; }
        public string Name { get; set; } = "";
        public bool IsPermitActive { get; set; }
        public bool MainPermit { get; set; }
        public bool Electrical { get; set; }
        public bool Mechanical1 { get; set; }
        public bool Mechanical2 { get; set; }
        public bool Operation { get; set; }
        // Used for tile colour
        public PermitType DominantPermit { get; set; }
        public bool IsClear =>
        !MainPermit &&
        !Electrical &&
        !Mechanical1 &&
        !Mechanical2 &&
        !Operation;
        public int ActivePermitTypeCount =>
            (Electrical ? 1 : 0) +
            (Mechanical1 ? 1 : 0) +
            (Mechanical2 ? 1 : 0) +
            (Operation ? 1 : 0);
    }
    public class PermitHistory
    {
        public int PermitId { get; set; }
        public string ConveyorName { get; set; } = "";
        public PermitType PermitType { get; set; }
        public string PermitName { get; set; } = "";
        public string WorkDescription { get; set; } = "";
        public DateTime IssuedTime { get; set; }
        public DateTime? ClearedTime { get; set; }
        public string Duration { get; set; } = "";
        public bool IsActive { get; set; }
        public string Status => IsActive ? "Active" : "Closed";
    }
    public class PermitTrend
    {
        public string Day { get; set; } = "";
        public double Electrical { get; set; }
        public double Mechanical1 { get; set; }
        public double Mechanical2 { get; set; }
        public double Operation { get; set; }
        public double Total =>
            Electrical +
            Mechanical1 +
            Mechanical2 +
            Operation;
    }
    public class TopPermitConveyor
    {
        public string Name { get; set; } = "";
        public double TotalHours { get; set; }
        public double ElectricalHours { get; set; }
        public double Mechanical1Hours { get; set; }
        public double Mechanical2Hours { get; set; }
        public double OperationHours { get; set; }
    }   
    public class PermitTypeStatistics
    {
        public int Electrical { get; set; }
        public int Mechanical1 { get; set; }
        public int Mechanical2 { get; set; }
        public int Operation { get; set; }
    }
    public class PermitLog
    {
        public int PermitTagId { get; set; }
        public int FieldDeviceId { get; set; }
        public string ConveyorName { get; set; } = "";
        public string PermitName { get; set; } = "";
        public PermitType Type { get; set; }
        public DateTime LogTime { get; set; }
        public DateTime? ResetTime { get; set; }
        public string Comment { get; set; } = "";
        public bool IsActive => !ResetTime.HasValue;
        public double DurationHours => ((ResetTime ?? DateTime.Now) - LogTime).TotalHours;
    }
}
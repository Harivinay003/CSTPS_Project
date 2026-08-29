using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;
using VirtualEMS.DataServices;
using VirtualEMS.Library;
using static System.Collections.Specialized.BitVector32;

namespace ManagementConsole
{
    public partial class Main : Form
    {
        int flag = 0;
        public static List<AlarmTag> AlarmTags = new List<AlarmTag>();
        public static List<AlarmParameter> AlarmParameters = new List<AlarmParameter>();
        public static List<IODevice> IODevices = new List<IODevice>();
        public static List<FieldDevice> FieldDevices = new List<FieldDevice>();
        public static List<SerialDevice> SerialDevices = new List<SerialDevice>();
        public static List<SerialDeviceDriver> SerialDeviceDrivers = new List<SerialDeviceDriver>();
        public static List<SerialDeviceParameter> SerialDeviceParameters = new List<SerialDeviceParameter>();
        public static List<SerialDeviceReadBlock> SerialDeviceReadBlocks = new List<SerialDeviceReadBlock>();
        public static List<SerialDeviceRegister> SerialDeviceRegisters = new List<SerialDeviceRegister>();
        public static List<Tag> Tags = new List<Tag>();
        public static List<TrendTag> TrendTags = new List<TrendTag>();
        public static List<TrendParameter> TrendParameters = new List<TrendParameter>();
        public static List<Permits> Permits = new List<Permits>();
        public static List<Group> Group = new List<Group>();
        public static string ConnectionString = string.Empty;
        private User _currentUser;

        //public Main()
        //{
        //    InitializeComponent();
        //}
        public Main(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }
        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["ConfigDBConnString"].ConnectionString;
                if (string.IsNullOrWhiteSpace(ConnectionString))
                {
                    MessageBox.Show("Connection string 'ConfigDBConnString' not found.", "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options;

                using var context = new AppDbContext(options);
                var repo = new dbRepository(context);

                // Populate static lists
                AlarmTags = repo.GetAlarmTags().ToList();
                AlarmParameters = repo.GetAlarmParameters().ToList();
                IODevices = repo.GetIODevices().ToList();
                FieldDevices = repo.GetFieldDevices().ToList();
                SerialDevices = repo.GetSerialDevices().ToList();
                SerialDeviceDrivers = repo.GetSerialDeviceDrivers().ToList();
                SerialDeviceParameters = repo.GetSerialDeviceParameters().ToList();
                SerialDeviceReadBlocks = repo.GetSerialDeviceReadBlocks().ToList();
                SerialDeviceRegisters = repo.GetSerialDeviceRegisters().ToList();
                Tags = repo.GetTags().ToList();
                TrendTags = repo.GetTrendTags().ToList();
                TrendParameters = repo.GetTrendParameters().ToList();
                Permits = repo.GetPermits().ToList();
                Group = repo.GetGroup().ToList();
                ApplyRoleAccess();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Default view
                btnEthernetDevbices.PerformClick();
            }
        }
        private void ApplyRoleAccess()
        {
            bool isAdmin = _currentUser.Username == "admin";

            btnDelete.Visible = isAdmin;
            button1.Visible = isAdmin;
        }
        private void btnEthernetDevbices_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Bold);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 0;
            dgView.DataSource = IODevices;
        }
        private void RefreshEthernetDevices()
        {
            using var context = CreateContext();

            IODevices = context.IODevices
                .ToList();
            ResetGrid();
            dgView.DataSource = IODevices;
        }
        private void btnFieldDevices_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Bold);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 1;
            var list = FieldDevices.Select(f => new
            {
                f.Id,
                f.Name,
                f.Description,
                IODevice = f.IODevice != null ? f.IODevice.Name : "",
                f.RunFBId
            }).ToList();
            dgView.DataSource = list;
        }
        private void RefreshFieldDevices()
        {
            using var context = CreateContext();

            FieldDevices = context.FieldDevices
                .Include(f => f.IODevice)
                .AsNoTracking()
                .ToList();

            ResetGrid();
            var list = FieldDevices.Select(f => new
            {
                f.Id,
                f.Name,
                f.Description,
                IODevice = f.IODevice != null ? f.IODevice.Name : "",
                f.RunFBId
            }).ToList();

            dgView.DataSource = list;
        }
        private void btnSerialDevices_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Bold);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 2;
            dgView.DataSource = SerialDevices;
            if (dgView.Columns["Driver"] != null)
            {
                dgView.Columns["Driver"].Visible = false;
            }
            if (dgView.Columns["Gateway"] != null)
            {
                dgView.Columns["Gateway"].Visible = false;
            }
        }
        private void RefreshSerialDevices()
        {
            using var context = CreateContext();

            SerialDevices = context.SerialDevices
                .Include(s => s.Driver)
                .Include(s => s.Gateway)
                .AsNoTracking()
                .ToList();

            ResetGrid();
            dgView.DataSource = SerialDevices;

            if (dgView.Columns["Driver"] != null)
                dgView.Columns["Driver"].Visible = false;

            if (dgView.Columns["Gateway"] != null)
                dgView.Columns["Gateway"].Visible = false;
        }
        private void btnDeviceTypes_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Bold);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 3;
            dgView.DataSource = SerialDeviceDrivers;
        }   
        private void RefreshDeviceTypes()
        {
            using var context = CreateContext();

            SerialDeviceDrivers = context.SerialDeviceDrivers
                .AsNoTracking()
                .ToList();

            SerialDeviceRegisters = context.SerialDeviceRegisters
                .AsNoTracking()
                .ToList();

            SerialDeviceReadBlocks = context.SerialDeviceReadBlocks
                .AsNoTracking()
                .ToList();

            ResetGrid();
            dgView.DataSource = SerialDeviceDrivers;
        }
        private void btnAlarm_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Bold);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 6;
            var alarmList = new List<AlarmViewModel>();

            using var context = CreateContext();

            AlarmParameters = context.AlarmParameters
                            .Include(a => a.SerialDevice)
                            .Include(a => a.SerialDeviceParameter)
                            .ToList();
            alarmList.AddRange(
                AlarmTags.Where(a => a.Category == EventCategory.Alarm)
                .Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.FieldDevice != null ? a.FieldDevice.Name : "",
                    Parameter = a.Tag != null ? a.Tag.Name : "",
                    Critical = (bool)a.Critical,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    LogOn = a.LogOn == true? "High": a.LogOn == false? "Low": "",
                    Type = "Tag",
                    Group = a.Group != null? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "": ""
                })
            );

            alarmList.AddRange(
                AlarmParameters
                .Where(a => a.Category == EventCategory.Alarm)
                .Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.SerialDevice != null ? a.SerialDevice.Name : "",
                    Parameter =  a.SerialDeviceParameter != null? a.SerialDeviceParameter.Name: "",
                    Critical = (bool)a.Critical,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    LogOn = a.LogOn == true? "High": a.LogOn == false? "Low": "",
                    Type = "Parameter",
                    Group = a.Group != null? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "": "",
                    Category = (EventCategory)a.Category
                })
            );
            dgView.DataSource = alarmList;
            if (dgView.Columns["Type"] != null)
            {
                dgView.Columns["Type"].Visible = false;
            }
            if (dgView.Columns["Category"] != null)
            {
                dgView.Columns["Category"].Visible = false;
            }
        }
        private void RefreshAlarms()
        {
            using var context = CreateContext();

            AlarmTags = context.AlarmTags
                        .Include(a => a.FieldDevice)
                        .Include(a => a.Tag)
                        .AsNoTracking()
                        .ToList();

            AlarmParameters = context.AlarmParameters
                .Include(a => a.SerialDevice)
                .Include(a => a.SerialDeviceParameter)
                .AsNoTracking()
                .ToList();

            ResetGrid();
            var alarmList = new List<AlarmViewModel>();
            AlarmParameters = context.AlarmParameters
                            .Include(a => a.SerialDevice)
                            .Include(a => a.SerialDeviceParameter)
                            .ToList();
            alarmList.AddRange(
                AlarmTags.Where(a => a.Category == EventCategory.Alarm)
                .Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.FieldDevice != null ? a.FieldDevice.Name : "",
                    Parameter = a.Tag != null ? a.Tag.Name : "",
                    Critical = (bool)a.Critical,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
                    Type = "Tag",
                    Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : ""
                })
            );

            alarmList.AddRange(
                AlarmParameters
                .Where(a => a.Category == EventCategory.Alarm)
                .Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.SerialDevice != null ? a.SerialDevice.Name : "",
                    Parameter = a.SerialDeviceParameter != null ? a.SerialDeviceParameter.Name : "",
                    Critical = (bool)a.Critical,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
                    Type = "Parameter",
                    Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : "",
                    Category = (EventCategory)a.Category
                })
            );
            dgView.DataSource = alarmList;
            if (dgView.Columns["Type"] != null)
            {
                dgView.Columns["Type"].Visible = false;
            }
            if (dgView.Columns["Category"] != null)
            {
                dgView.Columns["Category"].Visible = false;
            }
        }
        private void btnTags_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Bold);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 7;
            var list = Tags.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Type,
                t.Units,
                t.Address,
                t.Bit,
                t.Formula,
                t.SP_Min,
                t.SP_Max,
                t.Scale_Min,
                t.Scale_Max,
                t.StorageType,
                Device = t.Device != null ? t.Device.Name : "",
                FieldDevice = t.FieldDevice != null ? t.FieldDevice.Name : ""
            }).ToList();
            dgView.DataSource = list;
        }
        private void RefreshTags()
        {
            using var context = CreateContext();

            Tags = context.Tags
                .Include(t => t.Device)
                .Include(t => t.FieldDevice)
                .AsNoTracking()
                .ToList();

            ResetGrid();
            var list = Tags.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Type,
                t.Units,
                t.Address,
                t.Bit,
                t.Formula,
                t.SP_Min,
                t.SP_Max,
                t.Scale_Min,
                t.Scale_Max,
                t.StorageType,
                Device = t.Device != null ? t.Device.Name : "",
                FieldDevice = t.FieldDevice != null ? t.FieldDevice.Name : ""
            }).ToList();

            dgView.DataSource = list;
        }
        private void btnTrends_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Bold);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 8;

            using var context = CreateContext();

            TrendTags = context.TrendTags
                .Include(t => t.Tag)
                .Include(t => t.FieldDevice)
                .AsNoTracking()
                .ToList();

            TrendParameters = context.TrendParameters
                .Include(p => p.SerialDeviceParameter)
                .Include(p => p.SerialDevice)
                .AsNoTracking()
                .ToList();

            var tagGroups = TrendTags
                .GroupBy(t => new { t.FieldDeviceId, t.FieldDevice.Name })
                .Select(g => new TrendGroupView
                {
                    DeviceId = g.Key.FieldDeviceId,
                    Device = g.Key.Name,
                    Parameters = string.Join(", ", g.Select(x => x.Tag.Name)),
                    ParameterIds = g.Select(x => x.Id).ToList(),
                    Type = "Tag"
                });

            var paramGroups = TrendParameters
                .GroupBy(p => new { p.SerialDeviceId, p.SerialDevice.Name })
                .Select(g => new TrendGroupView
                {
                    DeviceId = g.Key.SerialDeviceId,
                    Device = g.Key.Name,
                    Parameters = string.Join(", ", g.Select(x => x.SerialDeviceParameter.Name)),
                    ParameterIds = g.Select(x => x.Id).ToList(),
                    Type = "Parameter"
                });

            dgView.DataSource = tagGroups.Concat(paramGroups).ToList();

            if (dgView.Columns["Type"] != null)
                dgView.Columns["Type"].Visible = false;

            if (dgView.Columns["DeviceId"] != null)
                dgView.Columns["DeviceId"].Visible = false;
        }
        private void RefreshTrends()
        {
            using var context = CreateContext();

            TrendTags = context.TrendTags
                .Include(t => t.Tag)
                .Include(t => t.FieldDevice)
                .AsNoTracking()
                .ToList();

            TrendParameters = context.TrendParameters
                .Include(p => p.SerialDeviceParameter)
                .Include(p => p.SerialDevice)
                .AsNoTracking()
                .ToList();

            var tagGroups = TrendTags
                .GroupBy(t => new { t.FieldDeviceId, t.FieldDevice.Name })
                .Select(g => new TrendGroupView
                {
                    DeviceId = g.Key.FieldDeviceId,
                    Device = g.Key.Name,
                    Parameters = string.Join(", ", g.Select(x => x.Tag.Name)),
                    ParameterIds = g.Select(x => x.Id).ToList(),
                    Type = "Tag"
                });

            var paramGroups = TrendParameters
                .GroupBy(p => new { p.SerialDeviceId, p.SerialDevice.Name })
                .Select(g => new TrendGroupView
                {
                    DeviceId = g.Key.SerialDeviceId,
                    Device = g.Key.Name,
                    Parameters = string.Join(", ", g.Select(x => x.SerialDeviceParameter.Name)),
                    ParameterIds = g.Select(x => x.Id).ToList(),
                    Type = "Parameter"
                });

            ResetGrid();
            dgView.DataSource = tagGroups.Concat(paramGroups).ToList();

            if (dgView.Columns["Type"] != null)
                dgView.Columns["Type"].Visible = false;

            if (dgView.Columns["DeviceId"] != null)
                dgView.Columns["DeviceId"].Visible = false;
        }
        private void btnEvents_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Bold);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 9;
            var EventList = new List<AlarmViewModel>();
            using var context = CreateContext();
            AlarmParameters = context.AlarmParameters
                .Include(t => t.SerialDevice)
                .Include(t => t.SerialDeviceParameter)
                .AsNoTracking()
                .ToList();
            AlarmTags = context.AlarmTags
                .Include(t => t.FieldDevice)
                .Include(t => t.Tag)
                .AsNoTracking()
                .ToList();
            EventList.AddRange(
    AlarmTags.Where(a => a.Category == EventCategory.Event).Select(a => new AlarmViewModel
    {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
        Device = a.FieldDevice != null ? a.FieldDevice.Name : "",
        Parameter = a.Tag != null ? a.Tag.Name : "",
        Critical = (bool)a.Critical,
        LowSetPoint = a.LowSetPoint,
        HighSetPoint = a.HighSetPoint,
        LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
        Changed = a.Changed,
        Type = "Tag",
        Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : ""
    })
);

            EventList.AddRange(
                AlarmParameters
                .Where(a => a.Category == EventCategory.Event)
                .Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.SerialDevice != null ? a.SerialDevice.Name : "",
                    Parameter = a.SerialDeviceParameter != null ? a.SerialDeviceParameter.Name : "",
                    Critical = (bool)a.Critical,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
                    Changed = a.Changed,
                    Type = "Parameter",
                    Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : "",
                    Category = (EventCategory)a.Category
                })
            );

            dgView.DataSource = EventList;

            if (dgView.Columns["Category"] != null)
            {
                dgView.Columns["Category"].Visible = false;
            }
            if (dgView.Columns["Type"] != null)
            {
                dgView.Columns["Type"].Visible = false;
            }

        }
        private void RefreshEvents()
        {
            var EventList = new List<AlarmViewModel>();
            using var context = CreateContext();
            AlarmParameters = context.AlarmParameters
                          .Include(t => t.SerialDevice)
                          .Include(t => t.SerialDeviceParameter)
                          .AsNoTracking()
                          .ToList();
            AlarmTags = context.AlarmTags
                .Include(t => t.FieldDevice)
                .Include(t => t.Tag)
                .AsNoTracking()
                .ToList();
            EventList.AddRange(
    AlarmTags.Where(a => a.Category == EventCategory.Event).Select(a => new AlarmViewModel
    {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
        Device = a.FieldDevice != null ? a.FieldDevice.Name : "",
        Parameter = a.Tag != null ? a.Tag.Name : "",
        Critical = (bool)a.Critical,
        LowSetPoint = a.LowSetPoint,
        HighSetPoint = a.HighSetPoint,
        LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
        Changed = a.Changed,
        Type = "Tag",
        Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : ""
    })
);

            EventList.AddRange(
                AlarmParameters
                .Where(a => a.Category == EventCategory.Event)
                .Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.SerialDevice != null ? a.SerialDevice.Name : "",
                    Parameter = a.SerialDeviceParameter != null ? a.SerialDeviceParameter.Name : "",
                    Critical = (bool)a.Critical,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
                    Changed = a.Changed,
                    Type = "Parameter",
                    Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : "",
                    Category = (EventCategory)a.Category
                })
            );

            dgView.DataSource = EventList;

            if (dgView.Columns["Category"] != null)
            {
                dgView.Columns["Category"].Visible = false;
            }
            if (dgView.Columns["Type"] != null)
            {
                dgView.Columns["Type"].Visible = false;
            }
        }
        private void btnTrips_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Bold);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 10;

            var TripsList = new List<AlarmViewModel>();
            using var context = CreateContext();
            AlarmParameters = context.AlarmParameters
                .Include(t => t.SerialDevice)
                .Include(t => t.SerialDeviceParameter)
                .AsNoTracking()
                .ToList();
            AlarmTags = context.AlarmTags
                .Include(t => t.FieldDevice)
                .Include(t => t.Tag)
                .AsNoTracking()
                .ToList();
            TripsList.AddRange(
    AlarmTags.Where(a => a.Category == EventCategory.Trip).Select(a => new AlarmViewModel
    {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
        Device = a.FieldDevice != null ? a.FieldDevice.Name : "",
        Parameter = a.Tag != null ? a.Tag.Name : "",
        Critical = (bool)a.Critical,
        LowSetPoint = a.LowSetPoint,
        HighSetPoint = a.HighSetPoint,
        LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
        Type = "Tag",
        Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : ""
    })
);

            TripsList.AddRange(
                AlarmParameters
                .Where(a => a.Category == EventCategory.Trip)
                .Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.SerialDevice != null ? a.SerialDevice.Name : "",
                    Parameter = a.SerialDeviceParameter != null ? a.SerialDeviceParameter.Name : "",
                    Critical = (bool)a.Critical,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
                    Type = "Parameter",
                    Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : "",
                    Category = (EventCategory)a.Category
                })
            );

            dgView.DataSource = TripsList;
            if (dgView.Columns["Category"] != null)
            {
                dgView.Columns["Category"].Visible = false;
            }
            if (dgView.Columns["Type"] != null)
            {
                dgView.Columns["Type"].Visible = false;
            }
        }
        private void RefreshTrips()
        {
            var TripsList = new List<AlarmViewModel>();
            using var context = CreateContext();
            AlarmParameters = context.AlarmParameters
                .Include(t => t.SerialDevice)
                .Include(t => t.SerialDeviceParameter)
                .AsNoTracking()
                .ToList();
            AlarmTags = context.AlarmTags
                .Include(t => t.FieldDevice)
                .Include(t => t.Tag)
                .AsNoTracking()
                .ToList();
            TripsList.AddRange(
    AlarmTags.Where(a => a.Category == EventCategory.Trip).Select(a => new AlarmViewModel
    {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
        Device = a.FieldDevice != null ? a.FieldDevice.Name : "",
        Parameter = a.Tag != null ? a.Tag.Name : "",
        Critical = (bool)a.Critical,
        LowSetPoint = a.LowSetPoint,
        HighSetPoint = a.HighSetPoint,
        LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
        Type = "Tag",
        Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : ""
    })
);

            TripsList.AddRange(
                AlarmParameters
                .Where(a => a.Category == EventCategory.Trip)
                .Select(a => new AlarmViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Device = a.SerialDevice != null ? a.SerialDevice.Name : "",
                    Parameter = a.SerialDeviceParameter != null ? a.SerialDeviceParameter.Name : "",
                    Critical = (bool)a.Critical,
                    LowSetPoint = a.LowSetPoint,
                    HighSetPoint = a.HighSetPoint,
                    LogOn = a.LogOn == true ? "High" : a.LogOn == false ? "Low" : "",
                    Type = "Parameter",
                    Group = a.Group != null ? Main.Group.FirstOrDefault(g => g.Id == a.Group)?.Name ?? "" : "",
                    Category = (EventCategory)a.Category
                })
            );

            dgView.DataSource = TripsList;
            if (dgView.Columns["Category"] != null)
            {
                dgView.Columns["Category"].Visible = false;
            }
            if (dgView.Columns["Type"] != null)
            {
                dgView.Columns["Type"].Visible = false;
            }
        }
        private void btnPermits_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Bold);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Regular);
            flag = 11;

            using var context = CreateContext();
            Permits = context.Permits
              .Include(t => t.Tag)
              .Include(t => t.FieldDevice)
              .AsNoTracking()
              .ToList();
            var list = Permits.Select(p => new
            {
                p.Id,
                FieldDevice = p.FieldDevice != null? p.FieldDevice.Name: "",
                Tag = p.Tag != null? p.Tag.Name: ""
            }).ToList();

            dgView.DataSource = list;
        }
        private void RefreshPermits()
        {
            using var context = CreateContext();
            Permits = context.Permits
                      .Include(p => p.FieldDevice)
                      .Include(p => p.Tag)
                      .AsNoTracking()
                      .ToList();
            ResetGrid();
            var list = Permits.Select(p => new
            {
                p.Id,
                FieldDevice = p.FieldDevice != null ? p.FieldDevice.Name : "",
                Tag = p.Tag != null ? p.Tag.Name : ""
            }).ToList();

            dgView.DataSource = list;
        }
        private void btnGroup_Click(object sender, EventArgs e)
        {
            ResetGrid();
            lblDeviceTypes.Font = new Font(lblDeviceTypes.Font, FontStyle.Regular);
            lblSerialDevices.Font = new Font(lblSerialDevices.Font, FontStyle.Regular);
            lblFieldDevices.Font = new Font(lblFieldDevices.Font, FontStyle.Regular);
            lblEthernetDevices.Font = new Font(lblEthernetDevices.Font, FontStyle.Regular);
            lbAlarms.Font = new Font(lbAlarms.Font, FontStyle.Regular);
            lbTags.Font = new Font(lbTags.Font, FontStyle.Regular);
            lbTrends.Font = new Font(lbTrends.Font, FontStyle.Regular);
            lbEvents.Font = new Font(lbEvents.Font, FontStyle.Regular);
            lbPermits.Font = new Font(lbPermits.Font, FontStyle.Regular);
            lbTrips.Font = new Font(lbTrips.Font, FontStyle.Regular);
            lbGroup.Font = new Font(lbGroup.Font, FontStyle.Bold);
            flag = 12;

            using var context = CreateContext();
            var list = Group.Select(t => new
            {
                t.Id,
                t.Name,
            }).ToList();
            dgView.DataSource = list;
        }
        private void RefreshGroup()
        {
            using var context = CreateContext();

            Group = context.Group.ToList();
            ResetGrid();
            var list = Group.Select(t => new
            {
                t.Id,
                t.Name,
            }).ToList();

            dgView.DataSource = list;
        }
        private void btnAddNew_Click(object sender, EventArgs e)
        {
            switch (flag)
            {
                case 0:
                    AddIODevice ethernetDevice = new AddIODevice();
                    if (ethernetDevice.ShowDialog() == DialogResult.OK)
                        RefreshEthernetDevices();
                    break;
                case 1:
                    AddFieldDevice addFieldDevice = new AddFieldDevice();
                    if (addFieldDevice.ShowDialog() == DialogResult.OK)
                        RefreshFieldDevices();
                    break;
                case 2:
                    AddSerialDevice serialDevice = new AddSerialDevice();
                    if (serialDevice.ShowDialog() == DialogResult.OK)
                        RefreshSerialDevices();
                    break;
                case 3:
                    AddDeviceType addDeviceType = new AddDeviceType();
                    if (addDeviceType.ShowDialog() == DialogResult.OK)
                        RefreshDeviceTypes();
                    break;
                case 6:
                    AddAlarm addAlarm = new AddAlarm(EventCategory.Alarm);
                    if (addAlarm.ShowDialog() == DialogResult.OK)
                        RefreshAlarms();
                    break;
                case 7:
                    AddTag addTag = new AddTag();
                    if (addTag.ShowDialog() == DialogResult.OK)
                        RefreshTags();
                    break;
                case 8:
                    AddTrend addTrend = new AddTrend();
                    if (addTrend.ShowDialog() == DialogResult.OK)
                        RefreshTrends();
                    break;

                case 9:
                    AddAlarm addEvents = new AddAlarm(EventCategory.Event);
                    if (addEvents.ShowDialog() == DialogResult.OK)
                        RefreshEvents();
                    break;

                case 10:
                    AddAlarm addTrips = new AddAlarm(EventCategory.Trip);
                    if (addTrips.ShowDialog() == DialogResult.OK)
                        RefreshTrips();
                    break;

                case 11:
                    AddPermits addPermits = new AddPermits();
                    if (addPermits.ShowDialog() == DialogResult.OK)
                        RefreshPermits();
                    break;

                case 12:
                    AddGroup addGroup = new AddGroup();
                    if (addGroup.ShowDialog() == DialogResult.OK)
                        RefreshGroup();
                    break;
            }
        }          
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a record to edit");
                return;
            }
            switch (flag)
            {
                case 0:
                    {
                        var device = dgView.SelectedRows[0].DataBoundItem as IODevice;
                        if (device == null) return;
                        using (var form = new AddIODevice(device))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshEthernetDevices();
                        }
                        break;
                    }
                case 1:
                    {
                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;
                        var fieldDevice = FieldDevices.First(f => f.Id == id);
                        if (fieldDevice == null) return;
                        using (var form = new AddFieldDevice(fieldDevice))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshFieldDevices();
                        }
                        break;
                    }
                case 2:
                    {
                        var serial = dgView.SelectedRows[0].DataBoundItem as SerialDevice;
                        if (serial == null) return;
                        using (var form = new AddSerialDevice(serial))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshSerialDevices();
                        }
                        break;
                    }
                case 3:
                    {
                        var driver = dgView.SelectedRows[0].DataBoundItem as SerialDeviceDriver;
                        if (driver == null) return;
                        using (var form = new AddDeviceType(driver))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshDeviceTypes();
                        }
                        break;
                    }
               
                case 6:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as AlarmViewModel;
                        if (selected == null) return;
                        if (selected.Type == "Tag")
                        {
                            var tag = AlarmTags.FirstOrDefault(a => a.Id == selected.Id);

                            if (tag == null) return;

                            AddAlarm form = new AddAlarm(tag);

                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshAlarms();
                        }
                        else if (selected.Type == "Parameter")
                        {
                            var param = AlarmParameters.FirstOrDefault(a => a.Id == selected.Id);

                            if (param == null) return;

                            AddAlarm form = new AddAlarm(param);

                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshAlarms();
                        }
                        break;
                    }
                case 7:
                    {
                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;
                        var tag = Tags.First(f => f.Id == id);
                        if (tag == null) return;
                        using (var form = new AddTag(tag))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshTags();
                        }
                        break;
                    }
                case 8:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as TrendGroupView;
                        if (selected == null) return;

                        if (selected.Type == "Tag")
                        {
                            var tags = TrendTags
                                .Where(t => t.FieldDeviceId == selected.DeviceId)
                                .ToList();

                            if (!tags.Any()) return;

                            AddTrend form = new AddTrend(tags);

                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshTrends();
                        }
                        else
                        {
                            var parameters = TrendParameters
                                .Where(p => p.SerialDeviceId == selected.DeviceId)
                                .ToList();

                            if (!parameters.Any()) return;

                            AddTrend form = new AddTrend(parameters);

                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshTrends();
                        }

                        break;
                    }

                case 9:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as AlarmViewModel;

                        if (selected == null)
                            return;

                        if (selected.Type == "Tag")
                        {
                            var tag = AlarmTags.FirstOrDefault(a =>
                                a.Id == selected.Id &&
                                a.Category == EventCategory.Event);

                            if (tag == null)
                                return;

                            using (var form = new AddAlarm(tag))
                            {
                                if (form.ShowDialog() == DialogResult.OK)
                                    RefreshEvents();
                            }
                        }
                        else if (selected.Type == "Parameter")
                        {
                            var parameter = AlarmParameters.FirstOrDefault(a =>
                                a.Id == selected.Id &&
                                a.Category == EventCategory.Event);

                            if (parameter == null)
                                return;

                            using (var form = new AddAlarm(parameter))
                            {
                                if (form.ShowDialog() == DialogResult.OK)
                                    RefreshEvents();
                            }
                        }

                        break;
                    }
                case 10:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as AlarmViewModel;

                        if (selected == null)
                            return;

                        if (selected.Type == "Tag")
                        {
                            var tag = AlarmTags.FirstOrDefault(a =>
                                a.Id == selected.Id &&
                                a.Category == EventCategory.Trip);

                            if (tag == null)
                                return;

                            using (var form = new AddAlarm(tag))
                            {
                                if (form.ShowDialog() == DialogResult.OK)
                                    RefreshTrips();
                            }
                        }
                        else if (selected.Type == "Parameter")
                        {
                            var parameter = AlarmParameters.FirstOrDefault(a =>
                                a.Id == selected.Id &&
                                a.Category == EventCategory.Trip);

                            if (parameter == null)
                                return;

                            using (var form = new AddAlarm(parameter))
                            {
                                if (form.ShowDialog() == DialogResult.OK)
                                    RefreshTrips();
                            }
                        }

                        break;
                    }
                case 11:
                    {

                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;

                        var permit = Permits.FirstOrDefault(p => p.Id == id);

                        if (permit == null)
                            return;

                        using (var form = new AddPermits(permit))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshPermits();
                        }

                        break;
                    }

                case 12:
                    {
                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;

                        var group = Group.FirstOrDefault(t => t.Id == id);

                        if (group == null)
                            return;

                        using (var form = new AddGroup(group))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                                RefreshGroup();
                        }

                        break;
                    }
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a record to delete");
                return;
            }

            using var context = CreateContext();

            switch (flag)
            {
                case 0:
                    {
                        var device = dgView.SelectedRows[0].DataBoundItem as IODevice;
                        if (device == null) return;

                        string msg = $"Delete IO Device?\n\nName: {device.Name}";
                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        // delete dependent field devices
                        var fieldDevices = context.FieldDevices.Where(f => f.IODeviceId == device.Id);
                        context.FieldDevices.RemoveRange(fieldDevices);

                        context.IODevices.Remove(device);
                        context.SaveChanges();

                        RefreshFieldDevices();
                        RefreshEthernetDevices();
                        break;
                    }

                case 1:
                    {
                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;

                        var field = context.FieldDevices.FirstOrDefault(f => f.Id == id);
                        if (field == null) return;

                        string msg = $"Delete Field Device?\n\nName: {field.Name}";
                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        // delete tags + trends + alarms
                        context.Tags.RemoveRange(context.Tags.Where(t => t.FieldDeviceId == id));
                        context.TrendTags.RemoveRange(context.TrendTags.Where(t => t.FieldDeviceId == id));
                        context.AlarmTags.RemoveRange(context.AlarmTags.Where(a => a.FieldDeviceId == id));

                        context.FieldDevices.Remove(field);
                        context.SaveChanges();

                        RefreshTags();
                        RefreshAlarms();
                        RefreshTrends();
                        RefreshFieldDevices();
                        break;
                    }

                case 2:
                    {
                        var serial = dgView.SelectedRows[0].DataBoundItem as SerialDevice;
                        if (serial == null) return;

                        string msg = $"Delete Serial Device?\n\nName: {serial.Name}";
                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        context.TrendParameters.RemoveRange(
                            context.TrendParameters.Where(p => p.SerialDeviceId == serial.Id));

                        context.AlarmParameters.RemoveRange(
                            context.AlarmParameters.Where(a => a.SerialDeviceId == serial.Id));

                        context.SerialDevices.Remove(serial);
                        context.SaveChanges();

                        RefreshAlarms();
                        RefreshTrends();
                        RefreshSerialDevices();
                        break;
                    }
                case 3:
                    {
                        var driver = dgView.SelectedRows[0].DataBoundItem as SerialDeviceDriver;

                        if (driver == null) return;

                        var serialDevices = context.SerialDevices
                            .Where(s => s.DriverId == driver.Id)
                            .ToList();

                        int count = serialDevices.Count;

                        string message =
                            $"Delete Driver?\n\n" +
                            $"Name: {driver.Name}\n" +
                            $"Linked Devices: {count}\n\n" +
                            $"This will delete all related devices and data.";

                        var confirm = MessageBox.Show(
                            message,
                            "Confirm Delete",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (confirm != DialogResult.Yes)
                            return;

                        var serialIds = serialDevices.Select(s => s.Id).ToList();

                        context.TrendParameters.RemoveRange(
                            context.TrendParameters.Where(p => serialIds.Contains(p.SerialDeviceId)));

                        context.AlarmParameters.RemoveRange(
                            context.AlarmParameters.Where(a => serialIds.Contains((int)a.SerialDeviceId)));

                        context.SerialDeviceRegisters.RemoveRange(
                            context.SerialDeviceRegisters.Where(r => r.DriverId == driver.Id));

                        context.SerialDeviceReadBlocks.RemoveRange(
                            context.SerialDeviceReadBlocks.Where(r => r.DriverId == driver.Id));

                        context.SerialDevices.RemoveRange(serialDevices);

                        context.SerialDeviceDrivers.Remove(driver);

                        context.SaveChanges();

                        RefreshTrends();
                        RefreshAlarms();
                        RefreshSerialDevices();
                        RefreshDeviceTypes();
                        break;
                    }

               
                case 6:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as AlarmViewModel;
                        if (selected == null) return;

                        string msg =
                            $"Delete Alarm?\n\nName: {selected.Name}\n" +
                            $"Device: {selected.Device}\nParameter: {selected.Parameter}";

                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        if (selected.Type == "Tag")
                            context.AlarmTags.Remove(context.AlarmTags.Find(selected.Id));
                        else
                            context.AlarmParameters.Remove(context.AlarmParameters.Find(selected.Id));

                        context.SaveChanges();
                        RefreshAlarms();
                        break;
                    }

                case 7:
                    {
                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;

                        var tag = context.Tags.FirstOrDefault(t => t.Id == id);
                        if (tag == null) return;

                        string msg = $"Delete Tag?\n\nName: {tag.Name}";
                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        context.TrendTags.RemoveRange(context.TrendTags.Where(t => t.TagId == id));
                        context.AlarmTags.RemoveRange(context.AlarmTags.Where(a => a.TagId == id));

                        context.Tags.Remove(tag);
                        context.SaveChanges();

                        RefreshTrends();
                        RefreshAlarms();
                        RefreshTags();
                        break;
                    }

                case 8:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as TrendGroupView;
                        if (selected == null) return;

                        string msg =
                            $"Delete Trend?\n\nDevice: {selected.Device}\n" +
                            $"Parameters:\n{selected.Parameters}";

                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                        if (selected.Type == "Tag")
                        {
                            context.TrendTags.RemoveRange(
                                context.TrendTags.Where(t => t.FieldDeviceId == selected.DeviceId));
                        }
                        else
                        {
                            context.TrendParameters.RemoveRange(
                                context.TrendParameters.Where(p => p.SerialDeviceId == selected.DeviceId));
                        }

                        context.SaveChanges();
                        RefreshTrends();
                        break;
                    }

                case 9:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as AlarmViewModel;

                        if (selected == null)
                            return;

                        string msg =
                            $"Delete Event?\n\n" +
                            $"Name: {selected.Name}\n" +
                            $"Device: {selected.Device}\n" +
                            $"Parameter: {selected.Parameter}";

                        if (MessageBox.Show(msg,
                            "Confirm Delete",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) != DialogResult.Yes)
                            return;

                        if (selected.Type == "Tag")
                        {
                            var tag = context.AlarmTags.FirstOrDefault(a =>
                                a.Id == selected.Id &&
                                a.Category == EventCategory.Event);

                            if (tag != null)
                                context.AlarmTags.Remove(tag);
                        }
                        else if (selected.Type == "Parameter")
                        {
                            var parameter = context.AlarmParameters.FirstOrDefault(a =>
                                a.Id == selected.Id &&
                                a.Category == EventCategory.Event);

                            if (parameter != null)
                                context.AlarmParameters.Remove(parameter);
                        }

                        context.SaveChanges();

                        RefreshEvents();

                        break;
                    }
                case 10:
                    {
                        var selected = dgView.SelectedRows[0].DataBoundItem as AlarmViewModel;

                        if (selected == null)
                            return;

                        string msg =
                            $"Delete Trip?\n\n" +
                            $"Name: {selected.Name}\n" +
                            $"Device: {selected.Device}\n" +
                            $"Parameter: {selected.Parameter}";

                        if (MessageBox.Show(msg,
                            "Confirm Delete",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) != DialogResult.Yes)
                            return;

                        if (selected.Type == "Tag")
                        {
                            var tag = context.AlarmTags.FirstOrDefault(a =>
                                a.Id == selected.Id &&
                                a.Category == EventCategory.Trip);

                            if (tag != null)
                                context.AlarmTags.Remove(tag);
                        }
                        else if (selected.Type == "Parameter")
                        {
                            var parameter = context.AlarmParameters.FirstOrDefault(a =>
                                a.Id == selected.Id &&
                                a.Category == EventCategory.Trip);

                            if (parameter != null)
                                context.AlarmParameters.Remove(parameter);
                        }

                        context.SaveChanges();

                        RefreshTrips();

                        break;
                    }

                case 11:
                    {

                        int id = (int)dgView.SelectedRows[0].Cells["Id"].Value;

                        var permit = context.Permits.Include(e => e.FieldDevice).Include(e => e.Tag).FirstOrDefault(p => p.Id == id);

                        if (permit == null)
                            return;

                        string msg =
                            $"Delete Permit?\n\n" +
                            $"Field Device: {(permit.FieldDevice != null ? permit.FieldDevice.Name : "")}\n" +
                            $"Tag: {(permit.Tag != null ? permit.Tag.Name : "")}";

                        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                            return;

                        context.Permits.Remove(permit);

                        context.SaveChanges();

                        RefreshPermits();

                        break;
                       
                    }
                case 12:
                    {
                        int id = (int)dgView.SelectedRows[0]
                            .Cells["Id"].Value;

                        var group = context.Group
                            .FirstOrDefault(g => g.Id == id);

                        if (group == null)
                            return;

                        string msg =
                            $"Delete Group?\n\nName: {group.Name}";

                        if (MessageBox.Show(msg,
                            "Confirm",
                            MessageBoxButtons.YesNo)
                            != DialogResult.Yes)
                            return;

                        // remove references
                        foreach (var a in context.AlarmTags.Where(a => a.Group == id))
                            a.Group = null;

                        foreach (var a in context.AlarmParameters.Where(a => a.Group == id))
                            a.Group = null;

                        context.Group.Remove(group);

                        context.SaveChanges();

                        RefreshAlarms();
                        RefreshEvents();
                        RefreshTrips();
                        RefreshGroup();

                        break;
                    }
                default:
                    MessageBox.Show("Delete not implemented for this section");
                    break;
            }
        }
        private void ResetGrid()
        {
            dgView.DataSource = null;
            dgView.Columns.Clear();
            dgView.AutoGenerateColumns = true;
        }
        private AppDbContext CreateContext()
        {
            return new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options);
        }   
        private void button1_Click(object sender, EventArgs e)
        {
            cmsUsers.Show(button1, new Point(0, button1.Height));
        }
        private void menuAddUser_Click(object sender, EventArgs e)
        {
            new AddUser().ShowDialog();
        }
        private void menuChangePassword_Click(object sender, EventArgs e)
        {
            new ChangePassword().ShowDialog();
        } 
    }
}

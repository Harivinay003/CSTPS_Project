using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Windows.Forms;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class AddAlarm : Form
    {
        private AlarmTag editTag;
        private AlarmParameter editParameter;
        private EventCategory defaultCategory;

        public AddAlarm(EventCategory category)
        {
            InitializeComponent();
            defaultCategory = category;
        }

        public AddAlarm(AlarmTag tag)
        {
            InitializeComponent();
            editTag = tag;
        }

        public AddAlarm(AlarmParameter parameter)
        {
            InitializeComponent();
            editParameter = parameter;
        }

        private AppDbContext CreateContext()
        {
            return new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options);
        }

        private void AddAlarm_Load(object sender, EventArgs e)
        {
            LoadDevices();
            LoadType();
            // Show only for Event
            EventCategory category;

            if (editTag != null)
            {
                category = (EventCategory)editTag.Category;
            }
            else if (editParameter != null)
            {
                category = (EventCategory)editParameter.Category;
            }
            else
            {
                category = defaultCategory;
            }

            bool isEvent = category == EventCategory.Event;

            if (defaultCategory != null)
            {
                cmbType.SelectedItem = defaultCategory;

                cmbType.Enabled = false;
            }
            if (editTag != null)
            {
                tbName.Text = editTag.Name;

                tbDescription.Text = editTag.Description;

                cmbType.SelectedItem = editTag.Category;

                cmbDevice.SelectedValue = editTag.FieldDeviceId;

                LoadParameters();

                cmbParameter.SelectedValue = editTag.TagId;

                tbLow.Text = editTag.LowSetPoint?.ToString();

                tbHigh.Text = editTag.HighSetPoint?.ToString();

                if (editTag.Group != null)
                {
                    cmbGroup.SelectedValue = editTag.Group;
                }

                cbCritical.Checked = (bool)editTag.Critical;
                rbHigh.Checked = editTag.LogOn == true;

                rbLow.Checked = editTag.LogOn == false;
            }

            if (editParameter != null)
            {
                tbName.Text = editParameter.Name;

                tbDescription.Text = editParameter.Description;

                cmbType.SelectedItem = editParameter.Category;

                //if (editParameter.FieldDeviceId != null)
                //{
                //    cmbDevice.SelectedValue = editParameter.FieldDeviceId;
                //}

                //else
                //{
                cmbDevice.SelectedValue = editParameter.SerialDeviceId;
                //}

                LoadParameters();

                //if (editParameter.TagId != null)
                //{
                //    cmbParameter.SelectedValue = editParameter.TagId;
                //}

                //else
                //{
                cmbParameter.SelectedValue = editParameter.SerialDeviceParameterId;
                //}

                tbLow.Text =
                    editParameter.LowSetPoint?.ToString();

                tbHigh.Text =
                    editParameter.HighSetPoint?.ToString();

                if (editParameter.Group != null)
                {
                    cmbGroup.SelectedValue = editParameter.Group;
                }

                cbCritical.Checked = (bool)editParameter.Critical;

                rbHigh.Checked = editParameter.LogOn == true;

                rbLow.Checked = editParameter.LogOn == false;
                //cbLogOn.Checked = (bool)editParameter.LogOn;
            }
            UpdateControls();
        }

        private void LoadType()
        {
            cmbType.DataSource = Enum.GetValues(typeof(EventCategory));
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void LoadDevices()
        {
            var devices = Main.FieldDevices
                .Select(d => new DeviceItem
                {
                    Id = d.Id,
                    Name = d.Name,
                    Type = "Field"
                })
                .Concat(Main.SerialDevices.Select(d => new DeviceItem
                {
                    Id = d.Id,
                    Name = d.Name,
                    Type = "Serial"
                }))
                .ToList();

            cmbDevice.DataSource = devices;
            cmbDevice.DisplayMember = "Name";
            cmbDevice.ValueMember = "Id";
        }
        private void cmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadParameters();
            UpdateControls();
        }
        private void cmbParameter_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }
        private void LoadParameters()
        {
            var device = cmbDevice.SelectedItem as DeviceItem;

            if (device == null) return;

            if (device.Type == "Field")
            {
                cmbParameter.DataSource = Main.Tags
                    .Where(t => t.FieldDeviceId == device.Id)
                    .ToList();

                cmbParameter.DisplayMember = "Name";
                cmbParameter.ValueMember = "Id";

                cmbGroup.DataSource = Main.Group.ToList();

                cmbGroup.DisplayMember = "Name";
                cmbGroup.ValueMember = "Id";
            }
            else
            {
                var serialDevice = Main.SerialDevices
                    .FirstOrDefault(d => d.Id == device.Id);

                if (serialDevice == null) return;

                var paramIds = Main.SerialDeviceRegisters
                    .Where(r => r.DriverId == serialDevice.DriverId)
                    .Select(r => r.ParameterId)
                    .Distinct()
                    .ToList();

                cmbParameter.DataSource = Main.SerialDeviceParameters
                    .Where(p => paramIds.Contains(p.Id))
                    .ToList();

                cmbParameter.DisplayMember = "Name";
                cmbParameter.ValueMember = "Id";

                cmbGroup.DataSource = Main.Group.ToList();

                cmbGroup.DisplayMember = "Name";
                cmbGroup.ValueMember = "Id";
            }
        }

        private void UpdateControls()
        {
            var device = cmbDevice.SelectedItem as DeviceItem;

            if (device == null)
                return;
            
            //EventCategory category = (EventCategory)cmbType.SelectedItem;
            if (defaultCategory == EventCategory.Event)
            {
                cmbGroup.Enabled = false;
            }
            if (device.Type == "Field")
            {
                var tag = cmbParameter.SelectedItem as Tag;

                if (tag == null)
                    return;

                // Type 4 or 5
                if (tag.Type == DataType.BOOL || tag.Type == DataType.WBOOL)
                {
                    tbLow.Enabled = false;
                    tbHigh.Enabled = false;
                    //cbLogOn.Enabled = true;
                    rbHigh.Enabled = true;
                    rbLow.Enabled = true;
                    tbLow.Text = "";
                    tbHigh.Text = "";
                }
                else
                {
                    tbLow.Enabled = true;
                    tbHigh.Enabled = true;

                    rbHigh.Enabled = false;
                    rbLow.Enabled = false;

                    rbHigh.Checked = false;
                    rbLow.Checked = false;
                    //cbLogOn.Enabled = false;
                    //cbLogOn.Checked = false;
                }
            }


            else
            {
                var parameter = cmbParameter.SelectedItem as SerialDeviceParameter;

                if (parameter == null)
                    return;

                tbLow.Enabled = true;
                tbHigh.Enabled = true;
                rbHigh.Enabled = false;
                rbLow.Enabled = false;

                rbHigh.Checked = false;
                rbLow.Checked = false;
                //cbLogOn.Enabled = false;
                //cbLogOn.Checked = false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using var context = CreateContext();

            var device = cmbDevice.SelectedItem as DeviceItem;

            if (device == null)
                return;

            EventCategory category = (EventCategory)cmbType.SelectedItem;

            bool isFieldAlarm = device.Type == "Field";

            if (isFieldAlarm)
            {
                AlarmTag alarm;

                if (editTag != null)
                {
                    alarm = context.AlarmTags.Find(editTag.Id);

                    if (alarm == null)
                        return;
                }
                else
                {
                    alarm = new AlarmTag();

                    context.AlarmTags.Add(alarm);
                }

                alarm.Name = tbName.Text.Trim();

                alarm.Description = tbDescription.Text.Trim();

                alarm.FieldDeviceId = device.Id;

                alarm.TagId = Convert.ToInt32(cmbParameter.SelectedValue);
                alarm.Category = category;

                alarm.Group = cmbGroup.SelectedValue != null ? Convert.ToInt32(cmbGroup.SelectedValue) : null;

                bool isBool = rbHigh.Enabled || rbLow.Enabled;

                if (isBool)
                {
                    alarm.LowSetPoint = null;

                    alarm.HighSetPoint = null;

                    alarm.LogOn = rbHigh.Checked;
                }
                else
                {
                    alarm.LowSetPoint =
                        string.IsNullOrWhiteSpace(tbLow.Text)
                        ? null
                        : float.Parse(tbLow.Text);

                    alarm.HighSetPoint =
                        string.IsNullOrWhiteSpace(tbHigh.Text)
                        ? null
                        : float.Parse(tbHigh.Text);


                    alarm.LogOn = null;
                }
                alarm.Critical = cbCritical.Checked;
                //alarm.LowSetPoint = string.IsNullOrWhiteSpace(tbLow.Text) ? null: float.Parse(tbLow.Text);
                //alarm.HighSetPoint = string.IsNullOrWhiteSpace(tbHigh.Text)? null: float.Parse(tbHigh.Text);
                //alarm.LogOn = null;
                //alarm.LogOn = rbHigh.Checked;
                //alarm.LogOn = cbLogOn.Checked;

            }

            else
            {
                AlarmParameter alarm;

                if (editParameter != null)
                {
                    alarm = context.AlarmParameters.Find(editParameter.Id);
                    if (alarm == null)
                        return;
                }
                else
                {
                    alarm = new AlarmParameter();
                    context.AlarmParameters.Add(alarm);
                }
                alarm.Name = tbName.Text.Trim();
                alarm.Description = tbDescription.Text.Trim();
                //if (device.Type == "Field")
                //{
                //    alarm.FieldDeviceId = device.Id;
                //    alarm.TagId = Convert.ToInt32(cmbParameter.SelectedValue);
                //    alarm.SerialDeviceId = null;
                //    alarm.SerialDeviceParameterId = null;
                //}
                //else
                //{
                alarm.SerialDeviceId = device.Id;
                alarm.SerialDeviceParameterId = Convert.ToInt32(cmbParameter.SelectedValue);
                //    alarm.FieldDeviceId = null;
                //    alarm.TagId = null;
                //}
                alarm.Group = cmbGroup.SelectedValue != null ? Convert.ToInt32(cmbGroup.SelectedValue) : null;
                alarm.Critical = cbCritical.Checked;
                alarm.Category = category;

                bool isBool = rbHigh.Enabled || rbLow.Enabled;

                if (isBool)
                {
                    alarm.LowSetPoint = null;
                    alarm.HighSetPoint = null;
                    alarm.LogOn = rbHigh.Checked; //High - 1 Low - 0
                }
                else
                {
                    alarm.LowSetPoint =
                        string.IsNullOrWhiteSpace(tbLow.Text)
                        ? null
                        : float.Parse(tbLow.Text);

                    alarm.HighSetPoint =
                        string.IsNullOrWhiteSpace(tbHigh.Text)
                        ? null
                        : float.Parse(tbHigh.Text);


                    alarm.LogOn = null;
                }
                //alarm.LowSetPoint = string.IsNullOrWhiteSpace(tbLow.Text)? null: float.Parse(tbLow.Text);
                //alarm.HighSetPoint = string.IsNullOrWhiteSpace(tbHigh.Text)? null: float.Parse(tbHigh.Text);
                //alarm.LogOn = cbLogOn.Checked;
                //alarm.LogOn = rbHigh.Checked;
            }

            context.SaveChanges();

            DialogResult = DialogResult.OK;

            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnGroup_Click(object sender, EventArgs e)
        {
            AddGroup addGroup = new AddGroup();
            if (addGroup.ShowDialog() == DialogResult.OK)
                RefreshGroup();
        }
        private void RefreshGroup()
        {
            using var context = CreateContext();

            cmbGroup.DataSource = context.Group.ToList();
            cmbGroup.DisplayMember = "Name";
            cmbGroup.ValueMember = "Id";
        }

       
    }
    public class DeviceItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class AddTrend : Form
    {
        private List<TrendTag> editTags;
        private List<TrendParameter> editParameters;

        public AddTrend()
        {
            InitializeComponent();
        }

        public AddTrend(List<TrendTag> tags)
        {
            InitializeComponent();
            editTags = tags;
        }

        public AddTrend(List<TrendParameter> parameters)
        {
            InitializeComponent();
            editParameters = parameters;
        }

        private void AddTrend_Load(object sender, EventArgs e)
        {
            LoadDevices();

            if (editTags != null && editTags.Any())
            {
                cmbSerialDevice.SelectedValue = editTags.First().FieldDeviceId;

                var device = cmbSerialDevice.SelectedItem as DeviceComboItem;
                if (device != null)
                    LoadItems(device);
            }
            else if (editParameters != null && editParameters.Any())
            {
                cmbSerialDevice.SelectedValue = editParameters.First().SerialDeviceId;

                var device = cmbSerialDevice.SelectedItem as DeviceComboItem;
                if (device != null)
                    LoadItems(device);
            }
        }

        private AppDbContext CreateContext()
        {
            return new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options);
        }

        private void LoadDevices()
        {
            var devices = new List<DeviceComboItem>();

            devices.AddRange(Main.FieldDevices.Select(d => new DeviceComboItem
            {
                Id = d.Id,
                Name = d.Name,
                Type = "Field"
            }));

            devices.AddRange(Main.SerialDevices.Select(d => new DeviceComboItem
            {
                Id = d.Id,
                Name = d.Name,
                Type = "Serial"
            }));

            cmbSerialDevice.DataSource = devices;
            cmbSerialDevice.DisplayMember = "Name";
            cmbSerialDevice.ValueMember = "Id";
        }

        private void cmbSerialDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            var device = cmbSerialDevice.SelectedItem as DeviceComboItem;

            if (device == null)
                return;

            LoadItems(device);
        }

        private void LoadItems(DeviceComboItem device)
        {
            flpItems.Controls.Clear();
            if (device.Type == "Field")
            {
                var tags = Main.Tags
                    .Where(t => t.FieldDeviceId == device.Id)
                    .ToList();

                foreach (var tag in tags)
                {
                    bool selected = editTags != null &&
                                    editTags.Any(t => t.TagId == tag.Id);

                    bool flag = editTags != null &&
                                editTags.Any(t => t.TagId == tag.Id && t.Flag);

                    AddItemRow(tag.Id, tag.Name, "Tag", selected, flag);
                }
            }
            else
            {
                var serialDevice = Main.SerialDevices
                    .FirstOrDefault(d => d.Id == device.Id);

                if (serialDevice == null)
                    return;

                var paramIds = Main.SerialDeviceRegisters
                    .Where(r => r.DriverId == serialDevice.DriverId)
                    .Select(r => r.ParameterId)
                    .Distinct()
                    .ToList();

                var parameters = Main.SerialDeviceParameters
                    .Where(p => paramIds.Contains(p.Id))
                    .ToList();

                foreach (var param in parameters)
                {
                    bool selected = editParameters != null &&
                                    editParameters.Any(p => p.SerialDeviceParameterId == param.Id);

                    bool flag = editParameters != null &&
                                editParameters.Any(p => p.SerialDeviceParameterId == param.Id && p.Flag);

                    AddItemRow(param.Id, param.Name, "Parameter", selected, flag);
                }
            }
        }

        private void AddItemRow(int id, string name, string type, bool isChecked = false, bool flag = false)
        {
            var panel = new Panel
            {
                Width = flpItems.Width - 30,
                Height = 28,
                Tag = type
            };

            var chkSelect = new CheckBox
            {
                Left = 2,
                Top = 3,
                Width = 15,
                Tag = id,
                Checked = isChecked
            };

            var lblName = new Label
            {
                Left = 18,
                Top = 6,
                Width = flpItems.Width - 200,
                Text = name,
                AutoEllipsis = true
            };

            var chkFlag = new CheckBox
            {
                Left = flpItems.Width - 170,
                Top = 3,
                Text = "Advanced",
                Checked = flag
            };

            // Auto toggle flag when parameter/tag selected
            chkSelect.CheckedChanged += (s, e) =>
            {
                chkFlag.Checked = chkSelect.Checked;
            };

            panel.Controls.Add(chkSelect);
            panel.Controls.Add(lblName);
            panel.Controls.Add(chkFlag);

            flpItems.Controls.Add(panel);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using var context = CreateContext();

            var device = cmbSerialDevice.SelectedItem as DeviceComboItem;

            if (device == null) return;

            // Remove old trends during edit
            if (editTags != null && editTags.Any())
            {
                context.TrendTags.RemoveRange(
                    context.TrendTags.Where(t => t.FieldDeviceId == device.Id)
                );
            }

            if (editParameters != null && editParameters.Any())
            {
                context.TrendParameters.RemoveRange(
                    context.TrendParameters.Where(t => t.SerialDeviceId == device.Id)
                );
            }

            foreach (Panel panel in flpItems.Controls)
            {
                var chkSelect = panel.Controls.OfType<CheckBox>().First();
                var chkFlag = panel.Controls.OfType<CheckBox>().Last();

                if (!chkSelect.Checked)
                    continue;

                int itemId = (int)chkSelect.Tag;
                bool flag = chkFlag.Checked;

                if (panel.Tag.ToString() == "Tag")
                {
                    context.TrendTags.Add(new TrendTag
                    {
                        FieldDeviceId = device.Id,
                        TagId = itemId,
                        Flag = flag
                    });
                }
                else
                {
                    context.TrendParameters.Add(new TrendParameter
                    {
                        SerialDeviceId = device.Id,
                        SerialDeviceParameterId = itemId,
                        Flag = flag
                    });
                }
            }

            context.SaveChanges();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        public class DeviceComboItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }
    }
}

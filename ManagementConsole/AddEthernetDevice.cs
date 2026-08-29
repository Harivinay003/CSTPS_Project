using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class AddIODevice : Form
    {
        private readonly FormMode _mode;
        private readonly IODevice _device;
        public AddIODevice()
        {
            InitializeComponent();
            _mode = FormMode.Add;
        }
        public AddIODevice(IODevice device)
        {
            InitializeComponent();
            _mode = FormMode.Edit;
            _device = device;
        }
        private void AddEthernetDevice_Load(object sender, EventArgs e)
        {
            cmbDeviceType.DataSource = Enum.GetValues(typeof(IODeviceType));

            if (_mode == FormMode.Edit && _device != null)
            {
                txtName.Text = _device.Name;
                txtDescription.Text = _device.Description;
                txtIpAddress.Text = _device.IpAddress;
                cmbDeviceType.SelectedItem = _device.DeviceType;
                btnCreate.Text = "Update";
            }
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_mode == FormMode.Add)
                    AddDevice();
                else
                    UpdateDevice();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ERROR");
            }
        }
        private void AddDevice()
        {
            var device = new IODevice
            {
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                IpAddress = txtIpAddress.Text.Trim(),
                DeviceType = (IODeviceType)cmbDeviceType.SelectedItem
                //DeviceType = IODeviceType.EthernetDevice
            };

            using var context = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(Main.ConnectionString)
                    .Options);

            context.IODevices.Add(device);
            context.SaveChanges();

            DialogResult = DialogResult.OK;
            Close();
        }
        private void UpdateDevice()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options;

            using (var context = new AppDbContext(options))
            {
                var device = context.IODevices.FirstOrDefault(d => d.Id == _device.Id);
                if (device == null) return;

                device.Name = txtName.Text.Trim();
                device.Description = txtDescription.Text.Trim();
                device.IpAddress = txtIpAddress.Text.Trim();
                device.DeviceType = (IODeviceType)cmbDeviceType.SelectedItem;

                context.SaveChanges();
            }

            _device.Name = txtName.Text.Trim();
            _device.Description = txtDescription.Text.Trim();
            _device.IpAddress = txtIpAddress.Text.Trim();
            _device.DeviceType = (IODeviceType)cmbDeviceType.SelectedItem;

        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}

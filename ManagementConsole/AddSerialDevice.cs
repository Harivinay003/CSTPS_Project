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
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class AddSerialDevice : Form
    {
        private readonly FormMode _mode;
        private readonly SerialDevice _serialDevice;
        private bool _isSaving = false;

        public AddSerialDevice()
        {
            InitializeComponent();
            _mode = FormMode.Add;
        }
        public AddSerialDevice(SerialDevice serialDevice)
        {
            InitializeComponent();
            _mode = FormMode.Edit;
            _serialDevice = serialDevice;

            txtName.Text = _serialDevice.Name;
            txtDescription.Text = _serialDevice.Description;
            cmbDevice.SelectedItem = _serialDevice.Driver;
            cmbGateway.SelectedItem = _serialDevice.Gateway;
            txtUnit.Text = _serialDevice.UnitId.ToString();
            chkSwapRegs.Checked = _serialDevice.SwapRegs;

            btnCreate.Text = "Update";
        }

        private void AddSerialDevice_Load(object sender, EventArgs e)
        { 
            cmbDevice.DataSource = Main.SerialDeviceDrivers;
            cmbDevice.DisplayMember = "Name";   
            cmbDevice.ValueMember = "Id";       

            cmbGateway.DataSource = Main.IODevices
                .Where(d => d.DeviceType == IODeviceType.Gateway)
                .ToList();
            cmbGateway.DisplayMember = "Name";
            cmbGateway.ValueMember = "Id";

            if (_mode == FormMode.Edit && _serialDevice != null)
            {
                cmbDevice.SelectedValue = _serialDevice.DriverId;
                cmbGateway.SelectedValue = _serialDevice.GatewayId;
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (_isSaving) return;
            _isSaving = true;

            if (!ValidateForm())
            {
                _isSaving = false;
                return;
            }

            if (_mode == FormMode.Add)
                AddToDb();
            else
                UpdateInDb();

            DialogResult = DialogResult.OK;
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required");
                return false;
            }

            if (!int.TryParse(txtUnit.Text, out _))
            {
                MessageBox.Show("UnitId must be a number");
                return false;
            }

            if (cmbDevice.SelectedItem == null)
            {
                MessageBox.Show("Select a driver");
                return false;
            }

            if (cmbGateway.SelectedItem == null)
            {
                MessageBox.Show("Select a gateway");
                return false;
            }

            return true;
        }

        private void AddToDb()
        {
            var device = new SerialDevice
            {
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                UnitId = int.Parse(txtUnit.Text),
                SwapRegs = chkSwapRegs.Checked,
                DriverId = Convert.ToInt32(cmbDevice.SelectedValue),
                GatewayId = Convert.ToInt32(cmbGateway.SelectedValue)
            };

            using var context = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(Main.ConnectionString)
                    .Options);

            context.SerialDevices.Add(device);
            context.SaveChanges();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdateInDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options;

            using (var context = new AppDbContext(options))
            {
                var dbDevice = context.SerialDevices
                    .Include(s => s.Driver)
                    .Include(s => s.Gateway)
                    .FirstOrDefault(s => s.Id == _serialDevice.Id);

                if (dbDevice == null) return;

                dbDevice.Name = txtName.Text.Trim();
                dbDevice.Description = txtDescription.Text.Trim();
                dbDevice.UnitId = int.Parse(txtUnit.Text);
                dbDevice.SwapRegs = chkSwapRegs.Checked;
                dbDevice.DriverId = Convert.ToInt32(cmbDevice.SelectedValue);
                dbDevice.GatewayId = Convert.ToInt32(cmbGateway.SelectedValue);

                context.SaveChanges();
            }

            _serialDevice.Name = txtName.Text.Trim();
            _serialDevice.Description = txtDescription.Text.Trim();
            _serialDevice.UnitId = int.Parse(txtUnit.Text);
            _serialDevice.SwapRegs = chkSwapRegs.Checked;
            _serialDevice.Driver = cmbDevice.SelectedItem as SerialDeviceDriver;
            _serialDevice.Gateway = cmbGateway.SelectedItem as IODevice;
        }
    }
}

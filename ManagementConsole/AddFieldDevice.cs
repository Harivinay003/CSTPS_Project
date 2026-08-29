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
    public partial class AddFieldDevice : Form
    {
        private readonly FormMode _mode;
        private readonly FieldDevice _device;
        private bool _isSaving = false;

        public AddFieldDevice()
        {
            InitializeComponent();
            _mode = FormMode.Add;
        }

        public AddFieldDevice(FieldDevice device)
        {
            InitializeComponent();
            _mode = FormMode.Edit;
            _device = device;
        }

        private void AddFieldDevice_Load(object sender, EventArgs e)
        {
            cmbIODevice.DataSource = null;
            cmbIODevice.DataSource = Main.IODevices;

            cmbIODevice.DisplayMember = "Name";
            cmbIODevice.ValueMember = "Id";

            if (_mode == FormMode.Edit && _device != null)
            {
                txtName.Text = _device.Name;
                txtDescription.Text = _device.Description;
                cmbIODevice.SelectedValue = _device.IODeviceId;
                txtRunFBId.Text = _device.RunFBId?.ToString();
                btnCreate.Text = "Update";
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
                AddFieldDeviceToDb();
            else
                UpdateFieldDeviceInDb();

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required");
                return false;
            }

            return true;
        }

        private void AddFieldDeviceToDb()
        {
            var device = new FieldDevice
            {
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                IODeviceId = (int)cmbIODevice.SelectedValue,
                RunFBId = string.IsNullOrWhiteSpace(txtRunFBId.Text)
            ? null
            : int.Parse(txtRunFBId.Text)
            };

            using var context = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(Main.ConnectionString)
                    .Options);

            context.FieldDevices.Add(device);
            context.SaveChanges();
        }

        private void UpdateFieldDeviceInDb()
        {
            using var context = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(Main.ConnectionString)
                    .Options);

            var dbDevice = context.FieldDevices.FirstOrDefault(d => d.Id == _device.Id);

            if (dbDevice == null) return;

            dbDevice.Name = txtName.Text.Trim();
            dbDevice.Description = txtDescription.Text.Trim();
            dbDevice.IODeviceId = (int)cmbIODevice.SelectedValue;

            dbDevice.RunFBId = string.IsNullOrWhiteSpace(txtRunFBId.Text)
                ? null
                : int.Parse(txtRunFBId.Text);

            context.SaveChanges();

            _device.Name = txtName.Text.Trim();
            _device.Description = txtDescription.Text.Trim();
            _device.IODeviceId = (int)cmbIODevice.SelectedValue;
            _device.RunFBId = dbDevice.RunFBId;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

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
    public partial class AddPermits : Form
    {
        private Permits editPermit;

        public AddPermits()
        {
            InitializeComponent();
        }
        public AddPermits(Permits permit)
        {
            InitializeComponent();
            editPermit = permit;
        }
        private AppDbContext CreateContext()
        {
            return new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options);
        }

        private void AddPermits_Load(object sender, EventArgs e)
        {
            LoadDevices();

            if (editPermit != null)
            {
                cmbDevice.SelectedValue = editPermit.FieldDeviceId;

                LoadTags(editPermit.FieldDeviceId);

                cmbParameter.SelectedValue = editPermit.TagId;

                btnSave.Text = "Update";
            }
        }

        private void LoadDevices()
        {
            cmbDevice.DataSource = Main.FieldDevices.ToList();
            cmbDevice.DisplayMember = "Name";
            cmbDevice.ValueMember = "Id";
        }

        private void LoadTags(int fieldDeviceId)
        {
            cmbParameter.DataSource = Main.Tags
                .Where(t => t.FieldDeviceId == fieldDeviceId)
                .ToList();

            cmbParameter.DisplayMember = "Name";
            cmbParameter.ValueMember = "Id";
        }

        private void cmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDevice.SelectedItem is not FieldDevice device)
                return;

            LoadTags(device.Id);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbDevice.SelectedItem == null)
            {
                MessageBox.Show("Select Field Device");
                return;
            }

            if (cmbParameter.SelectedItem == null)
            {
                MessageBox.Show("Select Tag");
                return;
            }

            using var context = CreateContext();

            Permits permit;

            if (editPermit == null)
            {
                permit = new Permits();

                context.Permits.Add(permit);
            }
            else
            {
                permit = context.Permits.FirstOrDefault(p => p.Id == editPermit.Id);

                if (permit == null)
                    return;
            }

            permit.FieldDeviceId = Convert.ToInt32(cmbDevice.SelectedValue);

            permit.TagId = Convert.ToInt32(cmbParameter.SelectedValue);

            context.SaveChanges();

            DialogResult = DialogResult.OK;

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

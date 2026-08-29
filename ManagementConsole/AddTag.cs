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
    public partial class AddTag : Form
    {
        private readonly FormMode _mode;
        private readonly Tag _tag;

        public AddTag()
        {
            InitializeComponent();
            _mode = FormMode.Add;
        }

        public AddTag(Tag tag)
        {
            InitializeComponent();
            _mode = FormMode.Edit;
            _tag = tag;
        }

        private void AddTag_Load(object sender, EventArgs e)
        {
            cmbType.DataSource = Enum.GetValues(typeof(DataType));
            cmbStorage.DataSource = Enum.GetValues(typeof(StorageType));

            cmbFieldDevice.DataSource = Main.FieldDevices;
            cmbFieldDevice.DisplayMember = "Name";
            cmbFieldDevice.ValueMember = "Id";

            cmbDevice.DataSource = Main.IODevices;
            cmbDevice.DisplayMember = "Name";
            cmbDevice.ValueMember = "Id";

            if (_mode == FormMode.Edit)
            {
                txtName.Text = _tag.Name;
                txtDescription.Text = _tag.Description;
                txtUnits.Text = _tag.Units;

                txtAddress.Text = _tag.Address.ToString();
                txtBit.Text = _tag.Bit.ToString();
                txtFormula.Text = _tag.Formula;

                txtSPMin.Text = _tag.SP_Min.ToString();
                txtSPMax.Text = _tag.SP_Max.ToString();
                txtScaleMin.Text = _tag.Scale_Min.ToString();
                txtScaleMax.Text = _tag.Scale_Max.ToString();

                cmbType.SelectedItem = _tag.Type;
                cmbStorage.SelectedItem = _tag.StorageType;

                cmbDevice.SelectedValue = _tag.DeviceId;
                cmbFieldDevice.SelectedValue = _tag.FieldDeviceId;

                btnSave.Text = "Update";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using var context = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options);

            Tag tag;

            if (_mode == FormMode.Add)
            {
                tag = new Tag();
                context.Tags.Add(tag);
            }
            else
            {
                tag = context.Tags.First(t => t.Id == _tag.Id);
            }

            tag.DeviceId = (int)cmbDevice.SelectedValue;
            tag.FieldDeviceId = (int)cmbFieldDevice.SelectedValue;

            tag.Name = txtName.Text.Trim();
            tag.Description = txtDescription.Text.Trim();
            tag.Units = txtUnits.Text.Trim();
            tag.Formula = txtFormula.Text.Trim();
            tag.Address = int.Parse(txtAddress.Text);
            //tag.Bit = int.Parse(txtBit.Text);
            //tag.SP_Min = float.Parse(txtSPMin.Text);
            //tag.SP_Max = float.Parse(txtSPMax.Text);
            //tag.Scale_Min = float.Parse(txtScaleMin.Text);
            //tag.Scale_Max = float.Parse(txtScaleMax.Text);
            tag.Bit = int.TryParse(txtBit.Text, out var bit) ? bit : 0;
            tag.SP_Min = float.TryParse(txtSPMin.Text, out var spMin) ? spMin : 0;
            tag.SP_Max = float.TryParse(txtSPMax.Text, out var spMax) ? spMax : 0;
            tag.Scale_Min = float.TryParse(txtScaleMin.Text, out var scaleMin) ? scaleMin : 0;
            tag.Scale_Max = float.TryParse(txtScaleMax.Text, out var scaleMax) ? scaleMax : 0;

            tag.Type = (DataType)cmbType.SelectedItem;
            tag.StorageType = (StorageType)cmbStorage.SelectedItem;

            context.SaveChanges();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btncancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

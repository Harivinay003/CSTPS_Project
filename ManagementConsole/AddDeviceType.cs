using FastMember;
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
    public partial class AddDeviceType : Form
    {
        private readonly FormMode _mode;
        private readonly SerialDeviceDriver _driver;
        private bool _isSaving = false;

        public AddDeviceType()
        {
            InitializeComponent();
            _mode = FormMode.Add;
        }
        public AddDeviceType(SerialDeviceDriver driver)
        {
            InitializeComponent();
            _mode = FormMode.Edit;
            _driver = driver;
        }
        private void AddDeviceType_Load(object sender, EventArgs e)
        {
            LoadRegistersGrid();
            LoadReadBlocksGrid();

            if (_mode == FormMode.Edit && _driver != null)
            {
                txtName.Text = _driver.Name;
                txtMake.Text = _driver.Make;
                txtModel.Text = _driver.Model;
                btnAdd.Text = "Update";

                LoadExistingRegisters();
                LoadExistingReadBlocks();
            }
        }
        private void LoadRegistersGrid()
        {
            var parameters = Main.SerialDeviceParameters.ToList();
            DataTable table = new DataTable();

            using (var reader = ObjectReader.Create(parameters))
                table.Load(reader);

            foreach (DataColumn col in table.Columns)
                col.ReadOnly = true;

            if (!table.Columns.Contains("Address"))
                table.Columns.Add("Address", typeof(int));

            dgRegisters.AutoGenerateColumns = true;
            dgRegisters.DataSource = table;

            var dataTypeCol = new DataGridViewComboBoxColumn
            {
                Name = "DataType",
                HeaderText = "Data Type",
                DataSource = Enum.GetNames(typeof(DataType))
            };

            dgRegisters.Columns.Add(dataTypeCol);

            foreach (DataGridViewRow row in dgRegisters.Rows)
                row.Cells["DataType"].Value = "REAL";
        }
        private void LoadReadBlocksGrid()
        {
            DataTable table = new DataTable();
            table.Columns.Add("StartAddress", typeof(int));
            table.Columns.Add("Count", typeof(short));

            dgReadBlocks.DataSource = table;
        }
        private void LoadExistingRegisters()
        {
            foreach (var reg in Main.SerialDeviceRegisters
                .Where(r => r.DriverId == _driver.Id))
            {
                foreach (DataGridViewRow row in dgRegisters.Rows)
                {
                    if (Convert.ToInt32(row.Cells["Id"].Value) == reg.ParameterId)
                    {
                        row.Cells["Address"].Value = reg.RegisterAddress;
                        row.Cells["DataType"].Value = reg.DataType.ToString();
                        break;
                    }
                }
            }
        }
        private void LoadExistingReadBlocks()
        {
            var table = dgReadBlocks.DataSource as DataTable;
            table.Rows.Clear();

            foreach (var rb in Main.SerialDeviceReadBlocks
                .Where(r => r.DriverId == _driver.Id))
            {
                table.Rows.Add(rb.StartAddress, rb.Count);
            }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_isSaving) return;
            _isSaving = true;

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required");
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
        private void AddToDb()
        {
            using var context = CreateContext();

            var driver = new SerialDeviceDriver
            {
                Name = txtName.Text.Trim(),
                Make = txtMake.Text.Trim(),
                Model = txtModel.Text.Trim()
            };

            context.SerialDeviceDrivers.Add(driver);
            context.SaveChanges();

            SaveRegisters(context, driver.Id);
            SaveReadBlocks(context, driver.Id);

            context.SaveChanges();
        }

        private void UpdateInDb()
        {
            using var context = CreateContext();

            var dbDriver = context.SerialDeviceDrivers
                .FirstOrDefault(d => d.Id == _driver.Id);

            if (dbDriver == null) return;

            dbDriver.Name = txtName.Text.Trim();
            dbDriver.Make = txtMake.Text.Trim();
            dbDriver.Model = txtModel.Text.Trim();

            // Update registers using proper update logic
            UpdateRegisters(context, dbDriver.Id);

            // Update read blocks using proper update logic
            UpdateReadBlocks(context, dbDriver.Id);

            context.SaveChanges();

            _driver.Name = dbDriver.Name;
            _driver.Make = dbDriver.Make;
            _driver.Model = dbDriver.Model;
        }

        private void SaveRegisters(AppDbContext context, int driverId)
        {
            foreach (DataGridViewRow row in dgRegisters.Rows)
            {
                if (row.IsNewRow) continue;
                var addressValue = row.Cells["Address"].Value;

                if (addressValue == null || addressValue == DBNull.Value)
                    continue;

                context.SerialDeviceRegisters.Add(new SerialDeviceRegister
                {
                    DriverId = driverId,
                    ParameterId = Convert.ToInt32(row.Cells["Id"].Value),
                    RegisterAddress = Convert.ToInt32(row.Cells["Address"].Value),
                    DataType = Enum.Parse<DataType>(
                        row.Cells["DataType"].Value.ToString())
                });
            }
        }

        private void UpdateRegisters(AppDbContext context, int driverId)
        {
            // Get existing registers from database
            var existingRegisters = context.SerialDeviceRegisters
                .Where(r => r.DriverId == driverId)
                .ToList();

            // Process grid rows
            foreach (DataGridViewRow row in dgRegisters.Rows)
            {
                if (row.IsNewRow) continue;
                var addressValue = row.Cells["Address"].Value;

                if (addressValue == null || addressValue == DBNull.Value)
                {
                    // Remove register if exists
                    var parameterId = Convert.ToInt32(row.Cells["Id"].Value);
                    var toRemove = existingRegisters.FirstOrDefault(r => r.ParameterId == parameterId);
                    if (toRemove != null)
                    {
                        context.SerialDeviceRegisters.Remove(toRemove);
                        existingRegisters.Remove(toRemove);
                    }
                    continue;
                }

                var paramId = Convert.ToInt32(row.Cells["Id"].Value);
                var address = Convert.ToInt32(row.Cells["Address"].Value);
                var dataType = Enum.Parse<DataType>(row.Cells["DataType"].Value.ToString());

                // Find existing register
                var existing = existingRegisters.FirstOrDefault(r => r.ParameterId == paramId);

                if (existing != null)
                {
                    // Update existing
                    existing.RegisterAddress = address;
                    existing.DataType = dataType;
                    existingRegisters.Remove(existing);
                }
                else
                {
                    // Add new
                    context.SerialDeviceRegisters.Add(new SerialDeviceRegister
                    {
                        DriverId = driverId,
                        ParameterId = paramId,
                        RegisterAddress = address,
                        DataType = dataType
                    });
                }
            }

            // Remove registers that are no longer in the grid
            context.SerialDeviceRegisters.RemoveRange(existingRegisters);
        }

        private void SaveReadBlocks(AppDbContext context, int driverId)
        {
            var table = dgReadBlocks.DataSource as DataTable;

            foreach (DataRow row in table.Rows)
            {
                context.SerialDeviceReadBlocks.Add(new SerialDeviceReadBlock
                {
                    DriverId = driverId,
                    StartAddress = Convert.ToInt32(row["StartAddress"]),
                    Count = Convert.ToInt16(row["Count"])
                });
            }
        }

        private void UpdateReadBlocks(AppDbContext context, int driverId)
        {
            var table = dgReadBlocks.DataSource as DataTable;

            // Get existing read blocks
            var existingBlocks = context.SerialDeviceReadBlocks
                .Where(r => r.DriverId == driverId)
                .ToList();

            var currentBlocks = new List<(int StartAddress, short Count)>();

            // Collect current blocks from grid
            foreach (DataRow row in table.Rows)
            {
                var startAddress = Convert.ToInt32(row["StartAddress"]);
                var count = Convert.ToInt16(row["Count"]);
                currentBlocks.Add((startAddress, count));
            }

            // Update or add blocks
            foreach (var (startAddress, count) in currentBlocks)
            {
                var existing = existingBlocks.FirstOrDefault(b =>
                    b.StartAddress == startAddress);

                if (existing != null)
                {
                    // Update existing
                    existing.Count = count;
                    existingBlocks.Remove(existing);
                }
                else
                {
                    // Add new
                    context.SerialDeviceReadBlocks.Add(new SerialDeviceReadBlock
                    {
                        DriverId = driverId,
                        StartAddress = startAddress,
                        Count = count
                    });
                }
            }

            // Remove blocks that are no longer in the grid
            context.SerialDeviceReadBlocks.RemoveRange(existingBlocks);
        }

        private void dgView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void dgRegisters_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= dgView_KeyPress;

            if (dgRegisters.CurrentCell.ColumnIndex >= 0)
            {
                if (e.Control is TextBox tb)
                    tb.KeyPress += dgView_KeyPress;
            }
        }

        private void dgReadBlocks_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= dgView_KeyPress;

            if (e.Control is TextBox tb)
                tb.KeyPress += dgView_KeyPress;
        }

        private AppDbContext CreateContext()
        {
            return new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(Main.ConnectionString)
                    .Options);
        }

       
    }
}

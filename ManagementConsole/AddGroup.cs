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
    public partial class AddGroup : Form
    {
        private Group editGroup;

        public AddGroup()
        {
            InitializeComponent();
        }
        public AddGroup(Group group)
        {
            InitializeComponent();

            editGroup = group;
        }

        private AppDbContext CreateContext()
        {
            return new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options);
        }

        private void AddGroup_Load(object sender, EventArgs e)
        {
            if (editGroup != null)
            {
                tbName.Text = editGroup.Name;

                btnSave.Text = "Update";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Enter Group Name");

                return;
            }

            using var context = CreateContext();

            Group group;

            if (editGroup == null)
            {
                group = new Group();

                context.Group.Add(group);
            }
            else
            {
                group = context.Group.FirstOrDefault(g => g.Id == editGroup.Id);

                if (group == null)
                    return;
            }

            group.Name = tbName.Text.Trim();

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

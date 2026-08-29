using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class AddUser : Form
    {
        public AddUser()
        {
            InitializeComponent();
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Enter username and password");
                return;
            }

            using (var context = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options))
            {
                if (context.Users.Any(u => u.Username == username))
                {
                    MessageBox.Show("Username already exists");
                    return;
                }

                var user = new User
                {
                    Username = username,
                    PasswordHash = HashPassword(password)
                };

                context.Users.Add(user);
                context.SaveChanges();
            }

            MessageBox.Show("User created successfully");
            this.Close();
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

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

namespace ManagementConsole
{
    public partial class ChangePassword : Form
    {
        public ChangePassword()
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
    private void btnChange_Click(object sender, EventArgs e)
        {
   
            string username = txtUsername.Text.Trim();
            //string oldPassword = txtOldPassword.Text.Trim();
            string newPassword = txtConfirmPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(confirmPassword) ||
                string.IsNullOrEmpty(newPassword) )
            {
                MessageBox.Show("All fields are required");
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("New password and confirm password do not match");
                return;
            }

            using (var context = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Main.ConnectionString)
                .Options)) 
            {
                var user = context.Users.FirstOrDefault(u => u.Username == username);

                if (user == null)
                {
                    MessageBox.Show("User not found");
                    return;
                }

                //string oldHash = HashPassword(oldPassword);

                //if (user.PasswordHash != oldHash)
                //{
                //    MessageBox.Show("Old password is incorrect");
                //    return;
                //}

                
                user.PasswordHash = HashPassword(newPassword);

                context.SaveChanges();
            }

            MessageBox.Show("Password changed successfully");
            this.Close();
        }
    
    }
}

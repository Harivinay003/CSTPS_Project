using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace ManagementConsole
{
    public partial class LoginForm : Form
    {
        public User LoggedInUser { get; private set; }
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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
        //private void btnLogin_Click(object sender, EventArgs e)
        //{
        //    var username = txtUsername.Text.Trim();
        //    var password = txtPassword.Text;

        //    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        //    {
        //        MessageBox.Show("Please enter username and password", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    // create db context
        //    //var conn = ConfigurationManager.ConnectionStrings["ConfigDBConnString"]?.ConnectionString;
        //    //if (string.IsNullOrWhiteSpace(conn))
        //    //{
        //    //    MessageBox.Show("Database connection not configured", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    //    return;
        //    //}

        //    //var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>()
        //    //    .UseSqlServer(conn)
        //    //    .Options;

        //    //using var context = new AppDbContext(options);

        //    //var repo = new dbRepository(context);
        //    //var user = repo.GetUserByUsername(username);

        //    //if (user == null)
        //    //{
        //    //    MessageBox.Show("Invalid username or password", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    //    return;
        //    //}

        //    //if (PasswordHelper.Verify(password, user.PasswordHash, user.PasswordSalt))
        //    if(username == "admin" && password == "Myhome@2026") // hard-coded for testing - replace with real validation
        //    {
        //        this.DialogResult = DialogResult.OK;
        //        this.Close();
        //        return;
        //    }

        //    MessageBox.Show("Invalid username or password", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //}

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter username and password", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conn = ConfigurationManager.ConnectionStrings["ConfigDBConnString"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(conn))
            {
                MessageBox.Show("Database connection not configured", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(conn)
                .Options;

            using var context = new AppDbContext(options);
            var repo = new dbRepository(context);

            var user = repo.GetUserByUsername(username);

            if (user == null)
            {
                MessageBox.Show("Invalid username or password", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string enteredHash = HashPassword(password);

            if (user.PasswordHash != enteredHash)
            {
                MessageBox.Show("Invalid username or password", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoggedInUser = user;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        // Optional change password feature - triggered externally or add a button to call this
        public bool ChangePassword(string username, string currentPassword, string newPassword, string adminValidationCode)
        {
            // require a hard-coded validation value for extra security as requested
            const string hardCodedValidation = "letmein123"; // change as needed
            if (adminValidationCode != hardCodedValidation)
                return false;

            var conn = ConfigurationManager.ConnectionStrings["ConfigDBConnString"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(conn))
                return false;

            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(conn)
                .Options;

            using var context = new AppDbContext(options);
            var repo = new dbRepository(context);
            var user = repo.GetUserByUsername(username);
            if (user == null) return false;

            // verify current password
            //if (!PasswordHelper.Verify(currentPassword, user.PasswordHash, user.PasswordSalt))
            //    return false;

            PasswordHelper.CreateHash(newPassword, out var hash, out var salt);
            user.PasswordHash = hash;
            //user.PasswordSalt = salt;
            repo.UpdateUser(user);
            return true;
        }
    }
}
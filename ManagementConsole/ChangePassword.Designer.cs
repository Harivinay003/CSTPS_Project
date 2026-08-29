namespace ManagementConsole
{
    partial class ChangePassword
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCancel = new Button();
            btnChange = new Button();
            txtNewPassword = new TextBox();
            txtUsername = new TextBox();
            lblPassword = new Label();
            lblUsername = new Label();
            txtConfirmPassword = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(278, 139);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 27);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnChange
            // 
            btnChange.Location = new Point(153, 139);
            btnChange.Name = "btnChange";
            btnChange.Size = new Size(119, 27);
            btnChange.TabIndex = 10;
            btnChange.Text = "Change Password";
            btnChange.UseVisualStyleBackColor = true;
            btnChange.Click += btnChange_Click;
            // 
            // txtNewPassword
            // 
            txtNewPassword.Location = new Point(153, 57);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.PasswordChar = '*';
            txtNewPassword.Size = new Size(200, 23);
            txtNewPassword.TabIndex = 9;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(153, 24);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(200, 23);
            txtUsername.TabIndex = 8;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(32, 60);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(84, 15);
            lblPassword.TabIndex = 7;
            lblPassword.Text = "New Password";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(32, 27);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(60, 15);
            lblUsername.TabIndex = 6;
            lblUsername.Text = "Username";
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Location = new Point(153, 95);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.PasswordChar = '*';
            txtConfirmPassword.Size = new Size(200, 23);
            txtConfirmPassword.TabIndex = 13;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(32, 98);
            label1.Name = "label1";
            label1.Size = new Size(104, 15);
            label1.TabIndex = 12;
            label1.Text = "Confirm Password";
            // 
            // ChangePassword
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(377, 169);
            Controls.Add(txtConfirmPassword);
            Controls.Add(label1);
            Controls.Add(btnCancel);
            Controls.Add(btnChange);
            Controls.Add(txtNewPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(lblUsername);
            Name = "ChangePassword";
            StartPosition = FormStartPosition.CenterParent;
            Text = "ChangePassword";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCancel;
        private Button btnChange;
        private TextBox txtNewPassword;
        private TextBox txtUsername;
        private Label lblPassword;
        private Label lblUsername;
        private TextBox txtConfirmPassword;
        private Label label1;
    }
}
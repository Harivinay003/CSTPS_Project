namespace ManagementConsole
{
    partial class AddGroup
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
            lbName = new Label();
            tbName = new TextBox();
            btnCancel = new Button();
            btnSave = new Button();
            SuspendLayout();
            // 
            // lbName
            // 
            lbName.Font = new Font("Segoe UI", 11F);
            lbName.Location = new Point(12, 40);
            lbName.Name = "lbName";
            lbName.Size = new Size(100, 23);
            lbName.TabIndex = 20;
            lbName.Text = "Name";
            lbName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tbName
            // 
            tbName.Location = new Point(144, 40);
            tbName.Name = "tbName";
            tbName.Size = new Size(183, 23);
            tbName.TabIndex = 19;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 11F);
            btnCancel.Location = new Point(247, 91);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 31);
            btnCancel.TabIndex = 26;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnSave
            // 
            btnSave.Font = new Font("Segoe UI", 11F);
            btnSave.Location = new Point(144, 91);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 31);
            btnSave.TabIndex = 25;
            btnSave.Text = "Create";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // AddGroup
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(345, 138);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(lbName);
            Controls.Add(tbName);
            Name = "AddGroup";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddGroup";
            Load += AddGroup_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbName;
        private TextBox tbName;
        private Button btnCancel;
        private Button btnSave;
    }
}
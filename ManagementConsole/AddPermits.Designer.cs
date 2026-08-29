namespace ManagementConsole
{
    partial class AddPermits
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
            btnSave = new Button();
            cmbDevice = new ComboBox();
            cmbParameter = new ComboBox();
            lbParameter = new Label();
            lbfieldDevice = new Label();
            SuspendLayout();
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 11F);
            btnCancel.Location = new Point(234, 141);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 31);
            btnCancel.TabIndex = 30;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnSave
            // 
            btnSave.Font = new Font("Segoe UI", 11F);
            btnSave.Location = new Point(131, 141);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 31);
            btnSave.TabIndex = 29;
            btnSave.Text = "Create";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // cmbDevice
            // 
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(131, 42);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(183, 23);
            cmbDevice.TabIndex = 28;
            cmbDevice.SelectedIndexChanged += cmbDevice_SelectedIndexChanged;
            // 
            // cmbParameter
            // 
            cmbParameter.FormattingEnabled = true;
            cmbParameter.Location = new Point(131, 93);
            cmbParameter.Name = "cmbParameter";
            cmbParameter.Size = new Size(183, 23);
            cmbParameter.TabIndex = 27;
            // 
            // lbParameter
            // 
            lbParameter.Font = new Font("Segoe UI", 11F);
            lbParameter.Location = new Point(12, 93);
            lbParameter.Name = "lbParameter";
            lbParameter.Size = new Size(100, 23);
            lbParameter.TabIndex = 26;
            lbParameter.Text = "Parameter";
            lbParameter.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbfieldDevice
            // 
            lbfieldDevice.Font = new Font("Segoe UI", 11F);
            lbfieldDevice.Location = new Point(12, 40);
            lbfieldDevice.Name = "lbfieldDevice";
            lbfieldDevice.Size = new Size(100, 23);
            lbfieldDevice.TabIndex = 25;
            lbfieldDevice.Text = "Device";
            lbfieldDevice.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // AddPermits
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(334, 185);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(cmbDevice);
            Controls.Add(cmbParameter);
            Controls.Add(lbParameter);
            Controls.Add(lbfieldDevice);
            Name = "AddPermits";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddPermits";
            Load += AddPermits_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button btnCancel;
        private Button btnSave;
        private ComboBox cmbDevice;
        private ComboBox cmbParameter;
        private Label lbParameter;
        private Label lbfieldDevice;
    }
}
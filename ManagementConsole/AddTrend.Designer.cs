namespace ManagementConsole
{
    partial class AddTrend
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
            lbSerialDevice = new Label();
            lbParameter = new Label();
            cmbSerialDevice = new ComboBox();
            btnCreate = new Button();
            btnCancel = new Button();
            flpItems = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // lbSerialDevice
            // 
            lbSerialDevice.Font = new Font("Segoe UI", 11F);
            lbSerialDevice.ImageAlign = ContentAlignment.MiddleLeft;
            lbSerialDevice.Location = new Point(12, 19);
            lbSerialDevice.Name = "lbSerialDevice";
            lbSerialDevice.Size = new Size(100, 23);
            lbSerialDevice.TabIndex = 3;
            lbSerialDevice.Text = "Device";
            lbSerialDevice.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbParameter
            // 
            lbParameter.Font = new Font("Segoe UI", 11F);
            lbParameter.ImageAlign = ContentAlignment.MiddleLeft;
            lbParameter.Location = new Point(12, 69);
            lbParameter.Name = "lbParameter";
            lbParameter.Size = new Size(100, 23);
            lbParameter.TabIndex = 4;
            lbParameter.Text = "Parameter";
            lbParameter.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cmbSerialDevice
            // 
            cmbSerialDevice.FormattingEnabled = true;
            cmbSerialDevice.Location = new Point(137, 21);
            cmbSerialDevice.Name = "cmbSerialDevice";
            cmbSerialDevice.Size = new Size(226, 23);
            cmbSerialDevice.TabIndex = 8;
            cmbSerialDevice.SelectedIndexChanged += cmbSerialDevice_SelectedIndexChanged;
            // 
            // btnCreate
            // 
            btnCreate.CausesValidation = false;
            btnCreate.DialogResult = DialogResult.OK;
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Location = new Point(137, 294);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 23);
            btnCreate.TabIndex = 10;
            btnCreate.Text = "CREATE";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.CausesValidation = false;
            btnCancel.DialogResult = DialogResult.OK;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(218, 294);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "CANCEL";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // flpItems
            // 
            flpItems.AutoScroll = true;
            flpItems.FlowDirection = FlowDirection.TopDown;
            flpItems.Location = new Point(137, 69);
            flpItems.Name = "flpItems";
            flpItems.Size = new Size(348, 219);
            flpItems.TabIndex = 13;
            flpItems.WrapContents = false;
            // 
            // AddTrend
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(533, 329);
            Controls.Add(flpItems);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(cmbSerialDevice);
            Controls.Add(lbParameter);
            Controls.Add(lbSerialDevice);
            Name = "AddTrend";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddTrend";
            Load += AddTrend_Load;
            ResumeLayout(false);
        }

        #endregion
        private Label lbSerialDevice;
        private Label lbParameter;
        private ComboBox cmbSerialDevice;
        private Button btnCreate;
        private Button btnCancel;
        private FlowLayoutPanel flpItems;
    }
}
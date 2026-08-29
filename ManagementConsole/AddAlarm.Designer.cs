namespace ManagementConsole
{
    partial class AddAlarm
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
            tbName = new TextBox();
            tbDescription = new TextBox();
            lbDescription = new Label();
            lbName = new Label();
            lbfieldDevice = new Label();
            lbParameter = new Label();
            cmbParameter = new ComboBox();
            cmbDevice = new ComboBox();
            btnSave = new Button();
            lbCritical = new Label();
            cbCritical = new CheckBox();
            btnCancel = new Button();
            btnGroup = new Button();
            cmbGroup = new ComboBox();
            lbLow = new Label();
            tbHigh = new TextBox();
            tbLow = new TextBox();
            label1 = new Label();
            lbHigh = new Label();
            cmbType = new ComboBox();
            label2 = new Label();
            lbLogOn = new Label();
            rbHigh = new RadioButton();
            rbLow = new RadioButton();
            SuspendLayout();
            // 
            // tbName
            // 
            tbName.Location = new Point(166, 22);
            tbName.Name = "tbName";
            tbName.Size = new Size(183, 23);
            tbName.TabIndex = 2;
            // 
            // tbDescription
            // 
            tbDescription.Location = new Point(166, 65);
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new Size(183, 23);
            tbDescription.TabIndex = 3;
            // 
            // lbDescription
            // 
            lbDescription.Font = new Font("Segoe UI", 11F);
            lbDescription.Location = new Point(12, 65);
            lbDescription.Name = "lbDescription";
            lbDescription.Size = new Size(100, 23);
            lbDescription.TabIndex = 4;
            lbDescription.Text = "Description";
            lbDescription.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbName
            // 
            lbName.Font = new Font("Segoe UI", 11F);
            lbName.Location = new Point(12, 22);
            lbName.Name = "lbName";
            lbName.Size = new Size(100, 23);
            lbName.TabIndex = 5;
            lbName.Text = "Name";
            lbName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbfieldDevice
            // 
            lbfieldDevice.Font = new Font("Segoe UI", 11F);
            lbfieldDevice.Location = new Point(12, 117);
            lbfieldDevice.Name = "lbfieldDevice";
            lbfieldDevice.Size = new Size(100, 23);
            lbfieldDevice.TabIndex = 9;
            lbfieldDevice.Text = "Device";
            lbfieldDevice.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbParameter
            // 
            lbParameter.Font = new Font("Segoe UI", 11F);
            lbParameter.Location = new Point(12, 164);
            lbParameter.Name = "lbParameter";
            lbParameter.Size = new Size(100, 23);
            lbParameter.TabIndex = 10;
            lbParameter.Text = " Parameter";
            lbParameter.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cmbParameter
            // 
            cmbParameter.FormattingEnabled = true;
            cmbParameter.Location = new Point(166, 164);
            cmbParameter.Name = "cmbParameter";
            cmbParameter.Size = new Size(183, 23);
            cmbParameter.TabIndex = 13;
            cmbParameter.SelectedIndexChanged += cmbParameter_SelectedIndexChanged;
            // 
            // cmbDevice
            // 
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(166, 117);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(183, 23);
            cmbDevice.TabIndex = 14;
            cmbDevice.SelectedIndexChanged += cmbDevice_SelectedIndexChanged;
            // 
            // btnSave
            // 
            btnSave.Font = new Font("Segoe UI", 11F);
            btnSave.Location = new Point(166, 469);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 31);
            btnSave.TabIndex = 19;
            btnSave.Text = "Create";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // lbCritical
            // 
            lbCritical.Font = new Font("Segoe UI", 11F);
            lbCritical.Location = new Point(12, 348);
            lbCritical.Name = "lbCritical";
            lbCritical.Size = new Size(100, 23);
            lbCritical.TabIndex = 20;
            lbCritical.Text = "Crictical";
            lbCritical.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cbCritical
            // 
            cbCritical.AutoSize = true;
            cbCritical.Location = new Point(166, 354);
            cbCritical.Name = "cbCritical";
            cbCritical.Size = new Size(15, 14);
            cbCritical.TabIndex = 21;
            cbCritical.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 11F);
            btnCancel.Location = new Point(269, 469);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 31);
            btnCancel.TabIndex = 22;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnGroup
            // 
            btnGroup.Font = new Font("Segoe UI", 11F);
            btnGroup.Location = new Point(355, 301);
            btnGroup.Name = "btnGroup";
            btnGroup.Size = new Size(90, 28);
            btnGroup.TabIndex = 27;
            btnGroup.Text = "Add group";
            btnGroup.UseVisualStyleBackColor = true;
            btnGroup.Click += btnGroup_Click;
            // 
            // cmbGroup
            // 
            cmbGroup.FormattingEnabled = true;
            cmbGroup.Location = new Point(166, 306);
            cmbGroup.Name = "cmbGroup";
            cmbGroup.Size = new Size(183, 23);
            cmbGroup.TabIndex = 26;
            // 
            // lbLow
            // 
            lbLow.Font = new Font("Segoe UI", 11F);
            lbLow.Location = new Point(12, 304);
            lbLow.Name = "lbLow";
            lbLow.Size = new Size(100, 23);
            lbLow.TabIndex = 25;
            lbLow.Text = "Group";
            lbLow.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tbHigh
            // 
            tbHigh.Location = new Point(166, 262);
            tbHigh.Name = "tbHigh";
            tbHigh.Size = new Size(183, 23);
            tbHigh.TabIndex = 31;
            // 
            // tbLow
            // 
            tbLow.Location = new Point(166, 213);
            tbLow.Name = "tbLow";
            tbLow.Size = new Size(183, 23);
            tbLow.TabIndex = 30;
            // 
            // label1
            // 
            label1.Font = new Font("Segoe UI", 11F);
            label1.Location = new Point(12, 211);
            label1.Name = "label1";
            label1.Size = new Size(100, 23);
            label1.TabIndex = 29;
            label1.Text = "Low Set Point";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbHigh
            // 
            lbHigh.Font = new Font("Segoe UI", 11F);
            lbHigh.Location = new Point(7, 260);
            lbHigh.Name = "lbHigh";
            lbHigh.Size = new Size(114, 23);
            lbHigh.TabIndex = 28;
            lbHigh.Text = "High Set Point";
            lbHigh.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cmbType
            // 
            cmbType.FormattingEnabled = true;
            cmbType.Location = new Point(166, 440);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(183, 23);
            cmbType.TabIndex = 33;
            cmbType.Visible = false;
            // 
            // label2
            // 
            label2.Font = new Font("Segoe UI", 11F);
            label2.Location = new Point(12, 440);
            label2.Name = "label2";
            label2.Size = new Size(100, 23);
            label2.TabIndex = 32;
            label2.Text = "Type";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label2.Visible = false;
            // 
            // lbLogOn
            // 
            lbLogOn.Font = new Font("Segoe UI", 11F);
            lbLogOn.Location = new Point(12, 393);
            lbLogOn.Name = "lbLogOn";
            lbLogOn.Size = new Size(100, 23);
            lbLogOn.TabIndex = 34;
            lbLogOn.Text = "Log On";
            lbLogOn.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // rbHigh
            // 
            rbHigh.AutoSize = true;
            rbHigh.Location = new Point(166, 397);
            rbHigh.Name = "rbHigh";
            rbHigh.Size = new Size(51, 19);
            rbHigh.TabIndex = 36;
            rbHigh.TabStop = true;
            rbHigh.Text = "High";
            rbHigh.UseVisualStyleBackColor = true;
            // 
            // rbLow
            // 
            rbLow.AutoSize = true;
            rbLow.Location = new Point(256, 397);
            rbLow.Name = "rbLow";
            rbLow.Size = new Size(47, 19);
            rbLow.TabIndex = 37;
            rbLow.TabStop = true;
            rbLow.Text = "Low";
            rbLow.UseVisualStyleBackColor = true;
            // 
            // AddAlarm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(454, 503);
            Controls.Add(rbLow);
            Controls.Add(rbHigh);
            Controls.Add(lbLogOn);
            Controls.Add(cmbType);
            Controls.Add(label2);
            Controls.Add(tbHigh);
            Controls.Add(tbLow);
            Controls.Add(label1);
            Controls.Add(lbHigh);
            Controls.Add(btnGroup);
            Controls.Add(cmbGroup);
            Controls.Add(lbLow);
            Controls.Add(btnCancel);
            Controls.Add(cbCritical);
            Controls.Add(lbCritical);
            Controls.Add(btnSave);
            Controls.Add(cmbDevice);
            Controls.Add(cmbParameter);
            Controls.Add(lbParameter);
            Controls.Add(lbfieldDevice);
            Controls.Add(lbName);
            Controls.Add(lbDescription);
            Controls.Add(tbDescription);
            Controls.Add(tbName);
            Name = "AddAlarm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddAlarm";
            Load += AddAlarm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox tbName;
        private TextBox tbDescription;
        private Label lbDescription;
        private Label lbName;
        private ComboBox comboBox1;
        private Label lbfieldDevice;
        private Label lbParameter;
        private ComboBox cmbSerialDevice;
        private ComboBox cmbParameter;
        private ComboBox cmbDevice;
        private Button btnSave;
        private Label lbCritical;
        private CheckBox cbCritical;
        private Button btnCancel;
        private Button btnGroup;
        private ComboBox cmbGroup;
        private Label lbLow;
        private TextBox tbHigh;
        private TextBox tbLow;
        private Label label1;
        private Label lbHigh;
        private ComboBox cmbType;
        private Label label2;
        private Label lbLogOn;
        private RadioButton rbHigh;
        private RadioButton rbLow;
    }
}
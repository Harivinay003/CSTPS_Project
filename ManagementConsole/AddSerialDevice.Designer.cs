namespace ManagementConsole
{
    partial class AddSerialDevice
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
            txtUnit = new TextBox();
            txtDescription = new TextBox();
            txtName = new TextBox();
            btnCancel = new Button();
            btnCreate = new Button();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            cmbDevice = new ComboBox();
            label5 = new Label();
            cmbGateway = new ComboBox();
            lbSwap = new Label();
            chkSwapRegs = new CheckBox();
            SuspendLayout();
            // 
            // txtUnit
            // 
            txtUnit.Location = new Point(146, 110);
            txtUnit.Name = "txtUnit";
            txtUnit.Size = new Size(169, 23);
            txtUnit.TabIndex = 17;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(146, 50);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(169, 23);
            txtDescription.TabIndex = 19;
            // 
            // txtName
            // 
            txtName.Location = new Point(146, 21);
            txtName.Name = "txtName";
            txtName.Size = new Size(169, 23);
            txtName.TabIndex = 20;
            // 
            // btnCancel
            // 
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(240, 219);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 15;
            btnCancel.Text = "CANCEL";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnCreate
            // 
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Location = new Point(146, 219);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 23);
            btnCreate.TabIndex = 16;
            btnCreate.Text = "CREATE";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(30, 116);
            label4.Name = "label4";
            label4.Size = new Size(52, 17);
            label4.TabIndex = 11;
            label4.Text = "Unit Id:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(30, 82);
            label3.Name = "label3";
            label3.Size = new Size(85, 17);
            label3.TabIndex = 12;
            label3.Text = "DeviceType:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(30, 53);
            label2.Name = "label2";
            label2.Size = new Size(84, 17);
            label2.TabIndex = 13;
            label2.Text = "Description:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(30, 24);
            label1.Name = "label1";
            label1.Size = new Size(52, 17);
            label1.TabIndex = 14;
            label1.Text = "Name:";
            // 
            // cmbDevice
            // 
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(146, 79);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(169, 25);
            cmbDevice.TabIndex = 21;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(30, 144);
            label5.Name = "label5";
            label5.Size = new Size(68, 17);
            label5.TabIndex = 12;
            label5.Text = "Gateway";
           
            // 
            // cmbGateway
            // 
            cmbGateway.FormattingEnabled = true;
            cmbGateway.Location = new Point(146, 139);
            cmbGateway.Name = "cmbGateway";
            cmbGateway.Size = new Size(169, 25);
            cmbGateway.TabIndex = 21;

            // 
            // lbSwap
            // 
            lbSwap.AutoSize = true;
            lbSwap.Location = new Point(30, 175);
            lbSwap.Name = "lbSwap";
            lbSwap.Size = new Size(82, 17);
            lbSwap.TabIndex = 22;
            lbSwap.Text = "Swap Regs:";
            // 
            // chkSwapRegs
            // 
            chkSwapRegs.AutoSize = true;
            chkSwapRegs.Location = new Point(146, 175);
            chkSwapRegs.Name = "chkSwapRegs";
            chkSwapRegs.Size = new Size(15, 14);
            chkSwapRegs.TabIndex = 23;
            chkSwapRegs.UseVisualStyleBackColor = true;
            // 
            // AddSerialDevice
            // 
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(399, 254);
            Controls.Add(chkSwapRegs);
            Controls.Add(lbSwap);
            Controls.Add(cmbGateway);
            Controls.Add(cmbDevice);
            Controls.Add(txtUnit);
            Controls.Add(txtDescription);
            Controls.Add(txtName);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Century Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "AddSerialDevice";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddSerialDevice";
            Load += AddSerialDevice_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtUnit;
        private TextBox txtDescription;
        private TextBox txtName;
        private Button btnCancel;
        private Button btnCreate;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private ComboBox cmbDevice;
        private Label label5;
        private ComboBox cmbGateway;
        private Label lbSwap;
        private CheckBox chkSwapRegs;
    }
}
namespace ManagementConsole
{
    partial class AddIODevice
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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnCreate = new Button();
            btnCancel = new Button();
            txtName = new TextBox();
            txtIpAddress = new TextBox();
            txtDescription = new TextBox();
            lbType = new Label();
            cmbDeviceType = new ComboBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(26, 12);
            label1.Name = "label1";
            label1.Size = new Size(52, 17);
            label1.TabIndex = 0;
            label1.Text = "Name:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(26, 41);
            label2.Name = "label2";
            label2.Size = new Size(84, 17);
            label2.TabIndex = 0;
            label2.Text = "Description:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(26, 70);
            label3.Name = "label3";
            label3.Size = new Size(73, 17);
            label3.TabIndex = 0;
            label3.Text = "IpAddress:";
            // 
            // btnCreate
            // 
            btnCreate.CausesValidation = false;
            btnCreate.DialogResult = DialogResult.OK;
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Location = new Point(142, 130);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 23);
            btnCreate.TabIndex = 1;
            btnCreate.Text = "CREATE";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // btnCancel
            // 
            btnCancel.CausesValidation = false;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(236, 130);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "CANCEL";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtName
            // 
            txtName.Location = new Point(142, 9);
            txtName.Name = "txtName";
            txtName.Size = new Size(169, 23);
            txtName.TabIndex = 2;
            // 
            // txtIpAddress
            // 
            txtIpAddress.Location = new Point(142, 67);
            txtIpAddress.Name = "txtIpAddress";
            txtIpAddress.Size = new Size(169, 23);
            txtIpAddress.TabIndex = 2;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(142, 38);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(169, 23);
            txtDescription.TabIndex = 2;
            // 
            // lbType
            // 
            lbType.AutoSize = true;
            lbType.Location = new Point(26, 102);
            lbType.Name = "lbType";
            lbType.Size = new Size(36, 17);
            lbType.TabIndex = 3;
            lbType.Text = "Type";
            // 
            // cmbDeviceType
            // 
            cmbDeviceType.FormattingEnabled = true;
            cmbDeviceType.Location = new Point(142, 99);
            cmbDeviceType.Name = "cmbDeviceType";
            cmbDeviceType.Size = new Size(169, 25);
            cmbDeviceType.TabIndex = 4;
            // 
            // AddIODevice
            // 
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoValidate = AutoValidate.Disable;
            ClientSize = new Size(345, 165);
            Controls.Add(cmbDeviceType);
            Controls.Add(lbType);
            Controls.Add(txtIpAddress);
            Controls.Add(txtDescription);
            Controls.Add(txtName);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Century Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "AddIODevice";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddIODevice";
            Load += AddEthernetDevice_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Button btnCreate;
        private Button btnCancel;
        private TextBox txtName;
        private TextBox txtIpAddress;
        private TextBox txtDescription;
        private Label lbType;
        private ComboBox cmbDeviceType;
    }
}
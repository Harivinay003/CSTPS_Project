namespace ManagementConsole
{
    partial class AddFieldDevice
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
            txtRunFBId = new TextBox();
            txtDescription = new TextBox();
            txtName = new TextBox();
            btnCancel = new Button();
            btnCreate = new Button();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            lbDevice = new Label();
            cmbIODevice = new ComboBox();
            SuspendLayout();
            // 
            // txtRunFBId
            // 
            txtRunFBId.Location = new Point(138, 64);
            txtRunFBId.Name = "txtRunFBId";
            txtRunFBId.Size = new Size(169, 23);
            txtRunFBId.TabIndex = 8;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(138, 35);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(169, 23);
            txtDescription.TabIndex = 9;
            // 
            // txtName
            // 
            txtName.Location = new Point(138, 6);
            txtName.Name = "txtName";
            txtName.Size = new Size(169, 23);
            txtName.TabIndex = 10;
            // 
            // btnCancel
            // 
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(232, 124);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "CANCEL";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnCreate
            // 
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Location = new Point(138, 124);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 23);
            btnCreate.TabIndex = 7;
            btnCreate.Text = "CREATE";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(22, 67);
            label3.Name = "label3";
            label3.Size = new Size(61, 17);
            label3.TabIndex = 3;
            label3.Text = "RunFBId:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(22, 38);
            label2.Name = "label2";
            label2.Size = new Size(84, 17);
            label2.TabIndex = 4;
            label2.Text = "Description:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 9);
            label1.Name = "label1";
            label1.Size = new Size(52, 17);
            label1.TabIndex = 5;
            label1.Text = "Name:";
            // 
            // lbDevice
            // 
            lbDevice.AutoSize = true;
            lbDevice.Location = new Point(22, 97);
            lbDevice.Name = "lbDevice";
            lbDevice.Size = new Size(71, 17);
            lbDevice.TabIndex = 11;
            lbDevice.Text = "IODevice:";
            // 
            // cmbIODevice
            // 
            cmbIODevice.FormattingEnabled = true;
            cmbIODevice.Location = new Point(138, 93);
            cmbIODevice.Name = "cmbIODevice";
            cmbIODevice.Size = new Size(169, 25);
            cmbIODevice.TabIndex = 12;
            // 
            // AddFieldDevice
            // 
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(326, 154);
            Controls.Add(cmbIODevice);
            Controls.Add(lbDevice);
            Controls.Add(txtRunFBId);
            Controls.Add(txtDescription);
            Controls.Add(txtName);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Century Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "AddFieldDevice";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddFieldDevice";
            Load += AddFieldDevice_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtRunFBId;
        private TextBox txtDescription;
        private TextBox txtName;
        private Button btnCancel;
        private Button btnCreate;
        private Label label3;
        private Label label2;
        private Label label1;
        private Label lbDevice;
        private ComboBox cmbIODevice;
    }
}
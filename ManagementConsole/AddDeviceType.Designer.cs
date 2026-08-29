namespace ManagementConsole
{
    partial class AddDeviceType
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            topPanel = new Panel();
            txtModel = new TextBox();
            label3 = new Label();
            txtMake = new TextBox();
            label2 = new Label();
            txtName = new TextBox();
            label1 = new Label();
            rightPanel = new Panel();
            dgReadBlocks = new DataGridView();
            label4 = new Label();
            centerPanel = new Panel();
            dgRegisters = new DataGridView();
            label5 = new Label();
            panel1 = new Panel();
            btnSave = new Button();
            btnAdd = new Button();
            topPanel.SuspendLayout();
            rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgReadBlocks).BeginInit();
            centerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgRegisters).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.Controls.Add(txtModel);
            topPanel.Controls.Add(label3);
            topPanel.Controls.Add(txtMake);
            topPanel.Controls.Add(label2);
            topPanel.Controls.Add(txtName);
            topPanel.Controls.Add(label1);
            topPanel.Dock = DockStyle.Top;
            topPanel.Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(975, 50);
            topPanel.TabIndex = 0;
            // 
            // txtModel
            // 
            txtModel.Location = new Point(675, 12);
            txtModel.Name = "txtModel";
            txtModel.Size = new Size(182, 22);
            txtModel.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(622, 15);
            label3.Name = "label3";
            label3.Size = new Size(49, 17);
            label3.TabIndex = 0;
            label3.Text = "Model:";
            // 
            // txtMake
            // 
            txtMake.Location = new Point(374, 12);
            txtMake.Name = "txtMake";
            txtMake.Size = new Size(182, 22);
            txtMake.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(321, 15);
            label2.Name = "label2";
            label2.Size = new Size(44, 17);
            label2.TabIndex = 0;
            label2.Text = "Make:";
            // 
            // txtName
            // 
            txtName.Location = new Point(77, 12);
            txtName.Name = "txtName";
            txtName.Size = new Size(182, 22);
            txtName.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(24, 15);
            label1.Name = "label1";
            label1.Size = new Size(47, 17);
            label1.TabIndex = 0;
            label1.Text = "Name:";
            // 
            // rightPanel
            // 
            rightPanel.Controls.Add(dgReadBlocks);
            rightPanel.Controls.Add(label4);
            rightPanel.Dock = DockStyle.Right;
            rightPanel.Font = new Font("Century Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            rightPanel.Location = new Point(607, 50);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(368, 447);
            rightPanel.TabIndex = 1;
            // 
            // dgReadBlocks
            // 
            dgReadBlocks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgReadBlocks.BackgroundColor = Color.White;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgReadBlocks.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgReadBlocks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgReadBlocks.Dock = DockStyle.Fill;
            dgReadBlocks.Location = new Point(0, 33);
            dgReadBlocks.Name = "dgReadBlocks";
            dgReadBlocks.Size = new Size(368, 414);
            dgReadBlocks.TabIndex = 3;
            dgReadBlocks.EditingControlShowing += dgReadBlocks_EditingControlShowing;
            // 
            // label4
            // 
            label4.Dock = DockStyle.Top;
            label4.Font = new Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.Location = new Point(0, 0);
            label4.Name = "label4";
            label4.Size = new Size(368, 33);
            label4.TabIndex = 0;
            label4.Text = "Read Blocks";
            label4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // centerPanel
            // 
            centerPanel.Controls.Add(dgRegisters);
            centerPanel.Controls.Add(label5);
            centerPanel.Dock = DockStyle.Fill;
            centerPanel.Location = new Point(0, 50);
            centerPanel.Name = "centerPanel";
            centerPanel.Size = new Size(607, 447);
            centerPanel.TabIndex = 2;
            // 
            // dgRegisters
            // 
            dgRegisters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgRegisters.BackgroundColor = Color.White;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgRegisters.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgRegisters.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgRegisters.Dock = DockStyle.Fill;
            dgRegisters.Location = new Point(0, 33);
            dgRegisters.Name = "dgRegisters";
            dgRegisters.Size = new Size(607, 414);
            dgRegisters.TabIndex = 2;
            dgRegisters.EditingControlShowing += dgRegisters_EditingControlShowing;
            dgRegisters.KeyPress += dgView_KeyPress;
            // 
            // label5
            // 
            label5.Dock = DockStyle.Top;
            label5.Font = new Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.Location = new Point(0, 0);
            label5.Name = "label5";
            label5.Size = new Size(607, 33);
            label5.TabIndex = 1;
            label5.Text = "Register Address";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnSave);
            panel1.Controls.Add(btnAdd);
            panel1.Dock = DockStyle.Bottom;
            panel1.Font = new Font("Century Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            panel1.Location = new Point(0, 497);
            panel1.Name = "panel1";
            panel1.Size = new Size(975, 44);
            panel1.TabIndex = 3;
            // 
            // btnSave
            // 
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Location = new Point(93, 10);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 0;
            btnSave.Text = "SAVE";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Location = new Point(12, 10);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(75, 23);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "ADD";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // AddDeviceType
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(975, 541);
            Controls.Add(centerPanel);
            Controls.Add(rightPanel);
            Controls.Add(topPanel);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "AddDeviceType";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AddDeviceType";
            Load += AddDeviceType_Load;
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            rightPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgReadBlocks).EndInit();
            centerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgRegisters).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel topPanel;
        private TextBox txtModel;
        private Label label3;
        private TextBox txtMake;
        private Label label2;
        private TextBox txtName;
        private Label label1;
        private Panel rightPanel;
        private Label label4;
        private Panel centerPanel;
        private Label label5;
        private DataGridView dgRegisters;
        private DataGridView dgReadBlocks;
        private Panel panel1;
        private Button btnSave;
        private Button btnAdd;
    }
}
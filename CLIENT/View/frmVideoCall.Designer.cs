namespace CLIENT.View
{
    partial class frmVideoCall
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
            components = new System.ComponentModel.Container();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnExit = new Button();
            contextMenuStrip1 = new ContextMenuStrip(components);
            mờiThamGiaToolStripMenuItem = new ToolStripMenuItem();
            thoátToolStripMenuItem = new ToolStripMenuItem();
            btnCamera = new Button();
            btnMicrophone = new Button();
            btnRecord = new Button();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(1081, 607);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.Gainsboro;
            btnExit.Location = new Point(12, 12);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(77, 43);
            btnExit.TabIndex = 1;
            btnExit.Text = "Thoát";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // mờiThamGiaToolStripMenuItem
            // 
            mờiThamGiaToolStripMenuItem.Name = "mờiThamGiaToolStripMenuItem";
            mờiThamGiaToolStripMenuItem.Size = new Size(32, 19);
            // 
            // thoátToolStripMenuItem
            // 
            thoátToolStripMenuItem.Name = "thoátToolStripMenuItem";
            thoátToolStripMenuItem.Size = new Size(32, 19);
            // 
            // btnCamera
            // 
            btnCamera.BackColor = Color.LightGreen;
            btnCamera.Location = new Point(991, 12);
            btnCamera.Name = "btnCamera";
            btnCamera.Size = new Size(77, 43);
            btnCamera.TabIndex = 2;
            btnCamera.Text = "Cam";
            btnCamera.UseVisualStyleBackColor = false;
            btnCamera.Click += btnCamera_Click;
            // 
            // btnMicrophone
            // 
            btnMicrophone.BackColor = Color.LightGreen;
            btnMicrophone.Location = new Point(908, 12);
            btnMicrophone.Name = "btnMicrophone";
            btnMicrophone.Size = new Size(77, 43);
            btnMicrophone.TabIndex = 3;
            btnMicrophone.Text = "Mic";
            btnMicrophone.UseVisualStyleBackColor = false;
            btnMicrophone.Click += btnMicrophone_Click;
            // 
            // btnRecord
            // 
            btnRecord.Location = new Point(95, 12);
            btnRecord.Name = "btnRecord";
            btnRecord.Size = new Size(86, 43);
            btnRecord.TabIndex = 4;
            btnRecord.Text = "Ghi hình";
            btnRecord.UseVisualStyleBackColor = true;
            btnRecord.Click += btnRecord_Click;
            // 
            // frmVideoCall
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1081, 607);
            Controls.Add(btnRecord);
            Controls.Add(btnMicrophone);
            Controls.Add(btnCamera);
            Controls.Add(btnExit);
            Controls.Add(tableLayoutPanel1);
            Name = "frmVideoCall";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Video Call";
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Button btnExit;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem mờiThamGiaToolStripMenuItem;
        private ToolStripMenuItem thoátToolStripMenuItem;
        private Button btnCamera;
        private Button btnMicrophone;
        private Button btnRecord;
    }
}
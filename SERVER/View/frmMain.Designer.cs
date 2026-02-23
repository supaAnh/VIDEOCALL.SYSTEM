namespace SERVER
{
    partial class frmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnExit = new Button();
            listViewHistory = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            btnConnect = new Button();
            label1 = new Label();
            textNumPort = new NumericUpDown();
            listViewClientConnected = new ListView();
            columnHeader3 = new ColumnHeader();
            btnClientOut = new Button();
            btnWatchRecord = new Button();
            ((System.ComponentModel.ISupportInitialize)textNumPort).BeginInit();
            SuspendLayout();
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.Red;
            btnExit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExit.ForeColor = Color.WhiteSmoke;
            btnExit.Location = new Point(12, 130);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(154, 57);
            btnExit.TabIndex = 0;
            btnExit.Text = "Ngắt kết nối";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // listViewHistory
            // 
            listViewHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewHistory.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listViewHistory.Font = new Font("Segoe UI", 10F);
            listViewHistory.Location = new Point(-2, 255);
            listViewHistory.Name = "listViewHistory";
            listViewHistory.Size = new Size(577, 290);
            listViewHistory.TabIndex = 1;
            listViewHistory.UseCompatibleStateImageBehavior = false;
            listViewHistory.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Thời gian";
            columnHeader1.Width = 110;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Hành động";
            columnHeader2.Width = 500;
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.FromArgb(41, 128, 185);
            btnConnect.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConnect.ForeColor = Color.White;
            btnConnect.Location = new Point(12, 67);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(154, 57);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "Mở kết nối";
            btnConnect.UseVisualStyleBackColor = false;
            btnConnect.Click += btnConnect_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.Location = new Point(12, 11);
            label1.Name = "label1";
            label1.Size = new Size(79, 20);
            label1.TabIndex = 4;
            label1.Text = "Chọn Port";
            // 
            // textNumPort
            // 
            textNumPort.Location = new Point(16, 34);
            textNumPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            textNumPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            textNumPort.Name = "textNumPort";
            textNumPort.Size = new Size(150, 27);
            textNumPort.TabIndex = 5;
            textNumPort.Value = new decimal(new int[] { 9000, 0, 0, 0 });
            // 
            // listViewClientConnected
            // 
            listViewClientConnected.Columns.AddRange(new ColumnHeader[] { columnHeader3 });
            listViewClientConnected.Location = new Point(206, 12);
            listViewClientConnected.Name = "listViewClientConnected";
            listViewClientConnected.Size = new Size(355, 175);
            listViewClientConnected.TabIndex = 6;
            listViewClientConnected.UseCompatibleStateImageBehavior = false;
            listViewClientConnected.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Người dùng đang Online";
            columnHeader3.Width = 350;
            // 
            // btnClientOut
            // 
            btnClientOut.BackColor = Color.Silver;
            btnClientOut.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnClientOut.ForeColor = Color.Red;
            btnClientOut.Location = new Point(411, 193);
            btnClientOut.Name = "btnClientOut";
            btnClientOut.Size = new Size(150, 56);
            btnClientOut.TabIndex = 7;
            btnClientOut.Text = "Xoá Client";
            btnClientOut.UseVisualStyleBackColor = false;
            btnClientOut.Click += btnClientOut_Click;
            // 
            // btnWatchRecord
            // 
            btnWatchRecord.Location = new Point(206, 194);
            btnWatchRecord.Name = "btnWatchRecord";
            btnWatchRecord.Size = new Size(150, 56);
            btnWatchRecord.TabIndex = 8;
            btnWatchRecord.Text = "Xem lại Record";
            btnWatchRecord.UseVisualStyleBackColor = true;
            btnWatchRecord.Click += btnWatchRecord_Click;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(573, 546);
            Controls.Add(btnWatchRecord);
            Controls.Add(btnClientOut);
            Controls.Add(listViewClientConnected);
            Controls.Add(textNumPort);
            Controls.Add(label1);
            Controls.Add(btnConnect);
            Controls.Add(listViewHistory);
            Controls.Add(btnExit);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SERVER";
            Load += frmMain_Load;
            ((System.ComponentModel.ISupportInitialize)textNumPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnExit;
        private ListView listViewHistory;
        private Button btnConnect;
        private Label label1;
        private NumericUpDown textNumPort;
        private ListView listViewClientConnected;
        private Button btnClientOut;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private Button btnWatchRecord;
    }
}

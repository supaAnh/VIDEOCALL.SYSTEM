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
            btnConnect = new Button();
            label1 = new Label();
            textNumPort = new NumericUpDown();
            listViewClientConnected = new ListView();
            btnClientOut = new Button();
            label2 = new Label();
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
            listViewHistory.Location = new Point(-2, 255);
            listViewHistory.Name = "listViewHistory";
            listViewHistory.Size = new Size(577, 290);
            listViewHistory.TabIndex = 1;
            listViewHistory.UseCompatibleStateImageBehavior = false;
            listViewHistory.View = View.Details;
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
            listViewClientConnected.Location = new Point(221, 37);
            listViewClientConnected.Name = "listViewClientConnected";
            listViewClientConnected.Size = new Size(340, 150);
            listViewClientConnected.TabIndex = 6;
            listViewClientConnected.UseCompatibleStateImageBehavior = false;
            listViewClientConnected.View = View.Details;
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
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label2.Location = new Point(221, 11);
            label2.Name = "label2";
            label2.Size = new Size(255, 23);
            label2.TabIndex = 8;
            label2.Text = "Danh sách Client đang kết nối:";
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(573, 546);
            Controls.Add(label2);
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
        private Label label2;
    }
}

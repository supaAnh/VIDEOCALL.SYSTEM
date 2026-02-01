namespace CLIENT
{
    partial class frmConnected
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
            textNumPort = new NumericUpDown();
            txtIPServer = new TextBox();
            btnConnect = new Button();
            label2 = new Label();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)textNumPort).BeginInit();
            SuspendLayout();
            // 
            // textNumPort
            // 
            textNumPort.Location = new Point(109, 18);
            textNumPort.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            textNumPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            textNumPort.Name = "textNumPort";
            textNumPort.Size = new Size(144, 27);
            textNumPort.TabIndex = 11;
            textNumPort.Value = new decimal(new int[] { 9000, 0, 0, 0 });
            // 
            // txtIPServer
            // 
            txtIPServer.Location = new Point(107, 60);
            txtIPServer.Name = "txtIPServer";
            txtIPServer.Size = new Size(146, 27);
            txtIPServer.TabIndex = 10;
            txtIPServer.Text = "127.0.0.1";
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.FromArgb(41, 128, 185);
            btnConnect.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConnect.ForeColor = Color.White;
            btnConnect.Location = new Point(107, 147);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(146, 59);
            btnConnect.TabIndex = 9;
            btnConnect.Text = "Kết nối Server";
            btnConnect.UseVisualStyleBackColor = false;
            btnConnect.Click += btnConnect_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F);
            label2.Location = new Point(12, 60);
            label2.Name = "label2";
            label2.Size = new Size(81, 23);
            label2.TabIndex = 8;
            label2.Text = "IP Server:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F);
            label1.Location = new Point(12, 18);
            label1.Name = "label1";
            label1.Size = new Size(91, 23);
            label1.TabIndex = 7;
            label1.Text = "Chọn Port:";
            // 
            // frmConnected
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(346, 218);
            Controls.Add(textNumPort);
            Controls.Add(txtIPServer);
            Controls.Add(btnConnect);
            Controls.Add(label2);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmConnected";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Kết nối";
            ((System.ComponentModel.ISupportInitialize)textNumPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NumericUpDown textNumPort;
        private TextBox txtIPServer;
        private Button btnConnect;
        private Label label2;
        private Label label1;
    }
}
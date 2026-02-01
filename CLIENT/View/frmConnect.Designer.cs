namespace CLIENT.View
{
    partial class frmConnect
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
            label1 = new Label();
            label2 = new Label();
            btnConnect = new Button();
            txtIPServer = new TextBox();
            textNumPort = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)textNumPort).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F);
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(91, 23);
            label1.TabIndex = 0;
            label1.Text = "Chọn Port:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F);
            label2.Location = new Point(12, 57);
            label2.Name = "label2";
            label2.Size = new Size(81, 23);
            label2.TabIndex = 1;
            label2.Text = "IP Server:";
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.FromArgb(41, 128, 185);
            btnConnect.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConnect.ForeColor = Color.White;
            btnConnect.Location = new Point(107, 144);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(146, 59);
            btnConnect.TabIndex = 3;
            btnConnect.Text = "Kết nối Server";
            btnConnect.UseVisualStyleBackColor = false;
            btnConnect.Click += btnConnect_Click;
            // 
            // txtIPServer
            // 
            txtIPServer.Location = new Point(107, 57);
            txtIPServer.Name = "txtIPServer";
            txtIPServer.Size = new Size(146, 27);
            txtIPServer.TabIndex = 5;
            txtIPServer.Text = "127.0.0.1";
            // 
            // textNumPort
            // 
            textNumPort.Location = new Point(109, 15);
            textNumPort.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            textNumPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            textNumPort.Name = "textNumPort";
            textNumPort.Size = new Size(144, 27);
            textNumPort.TabIndex = 6;
            textNumPort.Value = new decimal(new int[] { 9000, 0, 0, 0 });
            // 
            // frmConnect
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(346, 215);
            Controls.Add(textNumPort);
            Controls.Add(txtIPServer);
            Controls.Add(btnConnect);
            Controls.Add(label2);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmConnect";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Kết nối";
            Load += frmConnect_Load;
            ((System.ComponentModel.ISupportInitialize)textNumPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Button btnConnect;
        private TextBox txtIPServer;
        private NumericUpDown textNumPort;
    }
}

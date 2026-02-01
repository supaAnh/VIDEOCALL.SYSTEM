namespace CLIENT.View
{
    partial class frmLogin
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
            btnLogin = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            checkBox1 = new CheckBox();
            lbStatus = new Label();
            SuspendLayout();
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(159, 183);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(128, 41);
            btnLogin.TabIndex = 0;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.Location = new Point(159, 9);
            label1.Name = "label1";
            label1.Size = new Size(115, 28);
            label1.TabIndex = 1;
            label1.Text = "Đăng nhập";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F);
            label2.Location = new Point(20, 67);
            label2.Name = "label2";
            label2.Size = new Size(107, 20);
            label2.TabIndex = 2;
            label2.Text = "Tên đăng nhập";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 112);
            label3.Name = "label3";
            label3.Size = new Size(70, 20);
            label3.TabIndex = 3;
            label3.Text = "Mật khẩu";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(146, 64);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(164, 27);
            txtUsername.TabIndex = 4;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(146, 109);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(164, 27);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(146, 142);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(127, 24);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "HIện mật khẩu";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // lbStatus
            // 
            lbStatus.AutoSize = true;
            lbStatus.Location = new Point(119, 234);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new Size(191, 20);
            lbStatus.TabIndex = 7;
            lbStatus.Text = "Trạng thái: đang đăng nhập";
            // 
            // frmLogin
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(415, 263);
            Controls.Add(lbStatus);
            Controls.Add(checkBox1);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnLogin);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Đăng nhập";
            Load += frmLogin_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnLogin;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox checkBox1;
        private Label lbStatus;
    }
}
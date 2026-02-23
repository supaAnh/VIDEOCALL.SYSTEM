namespace SERVER.View
{
    partial class frmRecordManage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRecordManage));
            comboBoxRecordUser = new ComboBox();
            label1 = new Label();
            lbRecordList = new ListBox();
            axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            btnClose = new Button();
            btnChoose = new Button();
            ((System.ComponentModel.ISupportInitialize)axWindowsMediaPlayer1).BeginInit();
            SuspendLayout();
            // 
            // comboBoxRecordUser
            // 
            comboBoxRecordUser.FormattingEnabled = true;
            comboBoxRecordUser.Location = new Point(12, 64);
            comboBoxRecordUser.Name = "comboBoxRecordUser";
            comboBoxRecordUser.Size = new Size(286, 28);
            comboBoxRecordUser.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F);
            label1.Location = new Point(12, 23);
            label1.Name = "label1";
            label1.Size = new Size(145, 23);
            label1.TabIndex = 1;
            label1.Text = "Chọn người dùng";
            // 
            // lbRecordList
            // 
            lbRecordList.FormattingEnabled = true;
            lbRecordList.Location = new Point(12, 101);
            lbRecordList.Name = "lbRecordList";
            lbRecordList.Size = new Size(286, 364);
            lbRecordList.TabIndex = 2;
            // 
            // axWindowsMediaPlayer1
            // 
            axWindowsMediaPlayer1.Enabled = true;
            axWindowsMediaPlayer1.Location = new Point(304, 64);
            axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            axWindowsMediaPlayer1.OcxState = (AxHost.State)resources.GetObject("axWindowsMediaPlayer1.OcxState");
            axWindowsMediaPlayer1.Size = new Size(672, 461);
            axWindowsMediaPlayer1.TabIndex = 3;
            // 
            // btnClose
            // 
            btnClose.Font = new Font("Segoe UI", 10F);
            btnClose.Location = new Point(861, 12);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(115, 45);
            btnClose.TabIndex = 4;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnChoose
            // 
            btnChoose.Font = new Font("Segoe UI", 10F);
            btnChoose.Location = new Point(96, 480);
            btnChoose.Name = "btnChoose";
            btnChoose.Size = new Size(115, 45);
            btnChoose.TabIndex = 5;
            btnChoose.Text = "Chọn";
            btnChoose.UseVisualStyleBackColor = true;
    
            // 
            // frmRecordManage
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(988, 535);
            Controls.Add(btnChoose);
            Controls.Add(btnClose);
            Controls.Add(axWindowsMediaPlayer1);
            Controls.Add(lbRecordList);
            Controls.Add(label1);
            Controls.Add(comboBoxRecordUser);
            Name = "frmRecordManage";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SERVER - Quản lý Record";
            Load += FrmRecordManage_Load;
            ((System.ComponentModel.ISupportInitialize)axWindowsMediaPlayer1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox comboBoxRecordUser;
        private Label label1;
        private ListBox lbRecordList;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private Button btnClose;
        private Button btnChoose;
    }
}
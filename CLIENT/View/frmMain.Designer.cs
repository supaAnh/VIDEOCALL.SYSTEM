namespace CLIENT.View
{
    partial class frmMain
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
            lvOnlineUser = new ListView();
            columnHeader1 = new ColumnHeader();
            label1 = new Label();
            panelChooseChat = new Panel();
            btnSignOut = new Button();
            btnCreateGroupChat = new Button();
            btnChooseTarget = new Button();
            panelChatBox = new Panel();
            btnSelectionChatBox = new Button();
            btnCallVideo = new Button();
            lbTargetName = new Label();
            btnSendFile = new Button();
            btnSendChat = new Button();
            txtChat = new TextBox();
            labelUser = new Label();
            txtChatBox = new TextBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            thêmThànhViênToolStripMenuItem = new ToolStripMenuItem();
            xoáThànhViênToolStripMenuItem = new ToolStripMenuItem();
            xoáCuộcTròChuyệnToolStripMenuItem = new ToolStripMenuItem();
            openFileDialog1 = new OpenFileDialog();
            panelChooseChat.SuspendLayout();
            panelChatBox.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lvOnlineUser
            // 
            lvOnlineUser.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            lvOnlineUser.Location = new Point(13, 35);
            lvOnlineUser.Name = "lvOnlineUser";
            lvOnlineUser.Size = new Size(236, 348);
            lvOnlineUser.TabIndex = 0;
            lvOnlineUser.UseCompatibleStateImageBehavior = false;
            lvOnlineUser.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Đang trực tuyến";
            columnHeader1.Width = 250;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.Location = new Point(13, 9);
            label1.Name = "label1";
            label1.Size = new Size(124, 23);
            label1.TabIndex = 1;
            label1.Text = "Người liên hệ:";
            // 
            // panelChooseChat
            // 
            panelChooseChat.Controls.Add(btnSignOut);
            panelChooseChat.Controls.Add(btnCreateGroupChat);
            panelChooseChat.Controls.Add(btnChooseTarget);
            panelChooseChat.Controls.Add(label1);
            panelChooseChat.Controls.Add(lvOnlineUser);
            panelChooseChat.Location = new Point(729, 12);
            panelChooseChat.Name = "panelChooseChat";
            panelChooseChat.Size = new Size(262, 489);
            panelChooseChat.TabIndex = 2;
            // 
            // btnSignOut
            // 
            btnSignOut.Location = new Point(77, 436);
            btnSignOut.Name = "btnSignOut";
            btnSignOut.Size = new Size(110, 40);
            btnSignOut.TabIndex = 5;
            btnSignOut.Text = "Đăng xuất";
            btnSignOut.UseVisualStyleBackColor = true;
            // 
            // btnCreateGroupChat
            // 
            btnCreateGroupChat.Location = new Point(139, 389);
            btnCreateGroupChat.Name = "btnCreateGroupChat";
            btnCreateGroupChat.Size = new Size(110, 41);
            btnCreateGroupChat.TabIndex = 4;
            btnCreateGroupChat.Text = "Tạo nhóm";
            btnCreateGroupChat.UseVisualStyleBackColor = true;
            btnCreateGroupChat.Click += btnCreateGroupChat_Click;
            // 
            // btnChooseTarget
            // 
            btnChooseTarget.Location = new Point(13, 389);
            btnChooseTarget.Name = "btnChooseTarget";
            btnChooseTarget.Size = new Size(110, 41);
            btnChooseTarget.TabIndex = 3;
            btnChooseTarget.Text = "Trò chuyện";
            btnChooseTarget.UseVisualStyleBackColor = true;
            btnChooseTarget.Click += btnChooseTarget_Click;
            // 
            // panelChatBox
            // 
            panelChatBox.BackColor = Color.BlanchedAlmond;
            panelChatBox.Controls.Add(btnSelectionChatBox);
            panelChatBox.Controls.Add(btnCallVideo);
            panelChatBox.Controls.Add(lbTargetName);
            panelChatBox.Controls.Add(btnSendFile);
            panelChatBox.Controls.Add(btnSendChat);
            panelChatBox.Controls.Add(txtChat);
            panelChatBox.Controls.Add(labelUser);
            panelChatBox.Controls.Add(txtChatBox);
            panelChatBox.Location = new Point(12, 12);
            panelChatBox.Name = "panelChatBox";
            panelChatBox.Size = new Size(682, 489);
            panelChatBox.TabIndex = 3;
            // 
            // btnSelectionChatBox
            // 
            btnSelectionChatBox.BackColor = Color.OldLace;
            btnSelectionChatBox.Font = new Font("Segoe UI", 10F);
            btnSelectionChatBox.Location = new Point(567, 6);
            btnSelectionChatBox.Name = "btnSelectionChatBox";
            btnSelectionChatBox.Size = new Size(82, 46);
            btnSelectionChatBox.TabIndex = 7;
            btnSelectionChatBox.Text = "Cài đặt";
            btnSelectionChatBox.UseVisualStyleBackColor = false;
            btnSelectionChatBox.Click += btnSelectionChatBox_Click;
            // 
            // btnCallVideo
            // 
            btnCallVideo.BackColor = Color.OldLace;
            btnCallVideo.Font = new Font("Segoe UI", 10F);
            btnCallVideo.Location = new Point(479, 6);
            btnCallVideo.Name = "btnCallVideo";
            btnCallVideo.Size = new Size(82, 46);
            btnCallVideo.TabIndex = 6;
            btnCallVideo.Text = "Gọi";
            btnCallVideo.UseVisualStyleBackColor = false;
            btnCallVideo.Click += btnCallVideo_Click;
            // 
            // lbTargetName
            // 
            lbTargetName.AutoSize = true;
            lbTargetName.Font = new Font("Segoe UI", 10F);
            lbTargetName.ForeColor = SystemColors.InactiveCaptionText;
            lbTargetName.Location = new Point(91, 18);
            lbTargetName.Name = "lbTargetName";
            lbTargetName.Size = new Size(56, 23);
            lbTargetName.TabIndex = 5;
            lbTargetName.Text = "Name";
            // 
            // btnSendFile
            // 
            btnSendFile.BackColor = Color.OldLace;
            btnSendFile.Location = new Point(582, 425);
            btnSendFile.Name = "btnSendFile";
            btnSendFile.Size = new Size(67, 54);
            btnSendFile.TabIndex = 4;
            btnSendFile.Text = "File";
            btnSendFile.UseVisualStyleBackColor = false;
            // 
            // btnSendChat
            // 
            btnSendChat.BackColor = Color.OldLace;
            btnSendChat.Location = new Point(508, 425);
            btnSendChat.Name = "btnSendChat";
            btnSendChat.Size = new Size(67, 54);
            btnSendChat.TabIndex = 3;
            btnSendChat.Text = "Gửi";
            btnSendChat.UseVisualStyleBackColor = false;
            btnSendChat.Click += btnSendChat_Click;
            // 
            // txtChat
            // 
            txtChat.BackColor = Color.OldLace;
            txtChat.Location = new Point(32, 425);
            txtChat.Multiline = true;
            txtChat.Name = "txtChat";
            txtChat.Size = new Size(470, 54);
            txtChat.TabIndex = 2;
            // 
            // labelUser
            // 
            labelUser.AutoSize = true;
            labelUser.Font = new Font("Segoe UI", 10F);
            labelUser.Location = new Point(32, 18);
            labelUser.Name = "labelUser";
            labelUser.Size = new Size(53, 23);
            labelUser.TabIndex = 1;
            labelUser.Text = "User: ";
            // 
            // txtChatBox
            // 
            txtChatBox.BackColor = Color.OldLace;
            txtChatBox.Location = new Point(32, 55);
            txtChatBox.Multiline = true;
            txtChatBox.Name = "txtChatBox";
            txtChatBox.Size = new Size(617, 364);
            txtChatBox.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { thêmThànhViênToolStripMenuItem, xoáThànhViênToolStripMenuItem, xoáCuộcTròChuyệnToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(213, 76);
            // 
            // thêmThànhViênToolStripMenuItem
            // 
            thêmThànhViênToolStripMenuItem.Name = "thêmThànhViênToolStripMenuItem";
            thêmThànhViênToolStripMenuItem.Size = new Size(212, 24);
            thêmThànhViênToolStripMenuItem.Text = "Thêm thành viên";
            // 
            // xoáThànhViênToolStripMenuItem
            // 
            xoáThànhViênToolStripMenuItem.Name = "xoáThànhViênToolStripMenuItem";
            xoáThànhViênToolStripMenuItem.Size = new Size(212, 24);
            xoáThànhViênToolStripMenuItem.Text = "Xoá thành viên";
            // 
            // xoáCuộcTròChuyệnToolStripMenuItem
            // 
            xoáCuộcTròChuyệnToolStripMenuItem.Name = "xoáCuộcTròChuyệnToolStripMenuItem";
            xoáCuộcTròChuyệnToolStripMenuItem.Size = new Size(212, 24);
            xoáCuộcTròChuyệnToolStripMenuItem.Text = "Xoá cuộc trò chuyện";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1003, 509);
            Controls.Add(panelChatBox);
            Controls.Add(panelChooseChat);
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Main";
            Load += FrmMain_Load;
            panelChooseChat.ResumeLayout(false);
            panelChooseChat.PerformLayout();
            panelChatBox.ResumeLayout(false);
            panelChatBox.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ListView lvOnlineUser;
        private Label label1;
        private Panel panelChooseChat;
        private Button btnChooseTarget;
        private Button btnCreateGroupChat;
        private Button btnLogOut;
        private Panel panelChatBox;
        private TextBox txtChatBox;
        private Label lbTargetName;
        private Button btnSendFile;
        private Button btnSendChat;
        private TextBox txtChat;
        private Label labelUser;
        private Button btnCallVideo;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem thêmThànhViênToolStripMenuItem;
        private Button btnSelectionChatBox;
        private ToolStripMenuItem xoáThànhViênToolStripMenuItem;
        private ToolStripMenuItem xoáCuộcTròChuyệnToolStripMenuItem;
        private OpenFileDialog openFileDialog1;
        private Button btnSignOut;
        private ColumnHeader columnHeader1;
    }
}
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
            btnSelectionVideoCall = new Button();
            contextMenuStrip1 = new ContextMenuStrip(components);
            mờiThamGiaToolStripMenuItem = new ToolStripMenuItem();
            thoátToolStripMenuItem = new ToolStripMenuItem();
            btnCamera = new Button();
            btnMicrophone = new Button();
            contextMenuStrip1.SuspendLayout();
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
            // btnSelectionVideoCall
            // 
            btnSelectionVideoCall.BackColor = Color.Gainsboro;
            btnSelectionVideoCall.Location = new Point(12, 12);
            btnSelectionVideoCall.Name = "btnSelectionVideoCall";
            btnSelectionVideoCall.Size = new Size(77, 43);
            btnSelectionVideoCall.TabIndex = 1;
            btnSelectionVideoCall.Text = "Chọn";
            btnSelectionVideoCall.UseVisualStyleBackColor = false;
            btnSelectionVideoCall.Click += btnSelectionVideoCall_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { mờiThamGiaToolStripMenuItem, thoátToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(168, 52);
            // 
            // mờiThamGiaToolStripMenuItem
            // 
            mờiThamGiaToolStripMenuItem.Name = "mờiThamGiaToolStripMenuItem";
            mờiThamGiaToolStripMenuItem.Size = new Size(167, 24);
            mờiThamGiaToolStripMenuItem.Text = "Mời tham gia";
            // 
            // thoátToolStripMenuItem
            // 
            thoátToolStripMenuItem.Name = "thoátToolStripMenuItem";
            thoátToolStripMenuItem.Size = new Size(167, 24);
            thoátToolStripMenuItem.Text = "Thoát";
            thoátToolStripMenuItem.Click += thoátToolStripMenuItem_Click;
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
            // 
            // frmVideoCall
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1081, 607);
            Controls.Add(btnMicrophone);
            Controls.Add(btnCamera);
            Controls.Add(btnSelectionVideoCall);
            Controls.Add(tableLayoutPanel1);
            Name = "frmVideoCall";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Video Call";
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Button btnSelectionVideoCall;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem mờiThamGiaToolStripMenuItem;
        private ToolStripMenuItem thoátToolStripMenuItem;
        private Button btnCamera;
        private Button btnMicrophone;
    }
}
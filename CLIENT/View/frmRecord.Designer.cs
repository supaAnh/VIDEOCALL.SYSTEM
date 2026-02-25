namespace CLIENT.View
{
    partial class frmRecord
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRecord));
            lbRecordList = new ListBox();
            label1 = new Label();
            btnChoose = new Button();
            btnReturn = new Button();
            axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            ((System.ComponentModel.ISupportInitialize)axWindowsMediaPlayer1).BeginInit();
            SuspendLayout();
            // 
            // lbRecordList
            // 
            lbRecordList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbRecordList.FormattingEnabled = true;
            lbRecordList.Location = new Point(12, 66);
            lbRecordList.Name = "lbRecordList";
            lbRecordList.Size = new Size(308, 384);
            lbRecordList.TabIndex = 1;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F);
            label1.Location = new Point(12, 31);
            label1.Name = "label1";
            label1.Size = new Size(75, 23);
            label1.TabIndex = 2;
            label1.Text = "Xem lại: ";
            // 
            // btnChoose
            // 
            btnChoose.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnChoose.Location = new Point(110, 470);
            btnChoose.Name = "btnChoose";
            btnChoose.Size = new Size(94, 47);
            btnChoose.TabIndex = 3;
            btnChoose.Text = "Chọn";
            btnChoose.UseVisualStyleBackColor = true;
            // 
            // btnReturn
            // 
            btnReturn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnReturn.Location = new Point(887, 12);
            btnReturn.Name = "btnReturn";
            btnReturn.Size = new Size(94, 48);
            btnReturn.TabIndex = 4;
            btnReturn.Text = "Trở lại";
            btnReturn.UseVisualStyleBackColor = true;
            // 
            // axWindowsMediaPlayer1
            // 
            axWindowsMediaPlayer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            axWindowsMediaPlayer1.Enabled = true;
            axWindowsMediaPlayer1.Location = new Point(344, 66);
            axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            axWindowsMediaPlayer1.OcxState = (AxHost.State)resources.GetObject("axWindowsMediaPlayer1.OcxState");
            axWindowsMediaPlayer1.Size = new Size(637, 451);
            axWindowsMediaPlayer1.TabIndex = 5;
            // 
            // frmRecord
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(993, 529);
            Controls.Add(axWindowsMediaPlayer1);
            Controls.Add(btnReturn);
            Controls.Add(btnChoose);
            Controls.Add(label1);
            Controls.Add(lbRecordList);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Name = "frmRecord";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Record";
            Load += FrmRecord_Load;
            ((System.ComponentModel.ISupportInitialize)axWindowsMediaPlayer1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ListBox lbRecordList;
        private Label label1;
        private Button btnChoose;
        private Button btnReturn;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
    }
}
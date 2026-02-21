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
            pictureBoxRecord = new PictureBox();
            lbRecordList = new ListBox();
            label1 = new Label();
            btnChoose = new Button();
            btnReturn = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRecord).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxRecord
            // 
            pictureBoxRecord.Location = new Point(335, 66);
            pictureBoxRecord.Name = "pictureBoxRecord";
            pictureBoxRecord.Size = new Size(646, 451);
            pictureBoxRecord.TabIndex = 0;
            pictureBoxRecord.TabStop = false;
            // 
            // lbRecordList
            // 
            lbRecordList.FormattingEnabled = true;
            lbRecordList.Location = new Point(12, 66);
            lbRecordList.Name = "lbRecordList";
            lbRecordList.Size = new Size(308, 384);
            lbRecordList.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 31);
            label1.Name = "label1";
            label1.Size = new Size(66, 20);
            label1.TabIndex = 2;
            label1.Text = "Xem lại: ";
            // 
            // btnChoose
            // 
            btnChoose.Location = new Point(110, 470);
            btnChoose.Name = "btnChoose";
            btnChoose.Size = new Size(94, 47);
            btnChoose.TabIndex = 3;
            btnChoose.Text = "Chọn";
            btnChoose.UseVisualStyleBackColor = true;
            // 
            // btnReturn
            // 
            btnReturn.Location = new Point(887, 12);
            btnReturn.Name = "btnReturn";
            btnReturn.Size = new Size(94, 48);
            btnReturn.TabIndex = 4;
            btnReturn.Text = "Trở lại";
            btnReturn.UseVisualStyleBackColor = true;
            // 
            // frmRecord
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(993, 529);
            Controls.Add(btnReturn);
            Controls.Add(btnChoose);
            Controls.Add(label1);
            Controls.Add(lbRecordList);
            Controls.Add(pictureBoxRecord);
            Name = "frmRecord";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Record";
            ((System.ComponentModel.ISupportInitialize)pictureBoxRecord).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBoxRecord;
        private ListBox lbRecordList;
        private Label label1;
        private Button btnChoose;
        private Button btnReturn;
    }
}
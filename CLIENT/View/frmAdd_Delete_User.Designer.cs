namespace CLIENT.View
{
    partial class frmAdd_Delete_User
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
            checklb_ChooseUser = new CheckedListBox();
            btnChoose = new Button();
            btnExit = new Button();
            SuspendLayout();
            // 
            // checklb_ChooseUser
            // 
            checklb_ChooseUser.FormattingEnabled = true;
            checklb_ChooseUser.Location = new Point(12, 12);
            checklb_ChooseUser.Name = "checklb_ChooseUser";
            checklb_ChooseUser.Size = new Size(311, 290);
            checklb_ChooseUser.TabIndex = 4;
            // 
            // btnChoose
            // 
            btnChoose.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnChoose.Location = new Point(31, 308);
            btnChoose.Name = "btnChoose";
            btnChoose.Size = new Size(110, 48);
            btnChoose.TabIndex = 6;
            btnChoose.Text = "Chọn";
            btnChoose.UseVisualStyleBackColor = true;
            btnChoose.Click += btnChoose_Click;
            // 
            // btnExit
            // 
            btnExit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExit.Location = new Point(195, 308);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(110, 48);
            btnExit.TabIndex = 7;
            btnExit.Text = "Huỷ";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // frmAdd_Delete_User
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(336, 368);
            Controls.Add(btnExit);
            Controls.Add(btnChoose);
            Controls.Add(checklb_ChooseUser);
            Name = "frmAdd_Delete_User";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Thêm/Xoá User";
            ResumeLayout(false);
        }

        #endregion

        private CheckedListBox checklb_ChooseUser;
        private Button btnChoose;
        private Button btnExit;
    }
}
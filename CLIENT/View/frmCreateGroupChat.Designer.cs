namespace CLIENT.View
{
    partial class frmCreateGroupChat
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
            label1 = new Label();
            checklb_ChooseUser = new CheckedListBox();
            txtGroupName = new TextBox();
            btnCreateGroup = new Button();
            btnExit = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(12, 319);
            label1.Name = "label1";
            label1.Size = new Size(77, 20);
            label1.TabIndex = 1;
            label1.Text = "Tên nhóm:";
            // 
            // checklb_ChooseUser
            // 
            checklb_ChooseUser.FormattingEnabled = true;
            checklb_ChooseUser.Location = new Point(12, 12);
            checklb_ChooseUser.Name = "checklb_ChooseUser";
            checklb_ChooseUser.Size = new Size(311, 290);
            checklb_ChooseUser.TabIndex = 3;
            // 
            // txtGroupName
            // 
            txtGroupName.Location = new Point(95, 315);
            txtGroupName.Name = "txtGroupName";
            txtGroupName.Size = new Size(228, 27);
            txtGroupName.TabIndex = 4;
            // 
            // btnCreateGroup
            // 
            btnCreateGroup.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCreateGroup.Location = new Point(44, 365);
            btnCreateGroup.Name = "btnCreateGroup";
            btnCreateGroup.Size = new Size(110, 48);
            btnCreateGroup.TabIndex = 5;
            btnCreateGroup.Text = "Tạo";
            btnCreateGroup.UseVisualStyleBackColor = true;
            btnCreateGroup.Click += btnCreateGroup_Click;
            // 
            // btnExit
            // 
            btnExit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExit.Location = new Point(182, 365);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(110, 48);
            btnExit.TabIndex = 6;
            btnExit.Text = "Huỷ";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // frmCreateGroupChat
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(335, 427);
            Controls.Add(btnExit);
            Controls.Add(btnCreateGroup);
            Controls.Add(txtGroupName);
            Controls.Add(checklb_ChooseUser);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCreateGroupChat";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CLIENT - Tạo nhóm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private CheckedListBox checklb_ChooseUser;
        private TextBox txtGroupName;
        private Button btnCreateGroup;
        private Button btnExit;
    }
}
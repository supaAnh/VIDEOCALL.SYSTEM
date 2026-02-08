using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CLIENT.View
{
    public partial class frmCreateGroupChat : Form
    {

        public string GroupName { get; private set; }
        public List<string> SelectedUsers { get; private set; }

        public frmCreateGroupChat(List<string> onlineUsers)
        {
            InitializeComponent();

            foreach (var user in onlineUsers)
            {
                checklb_ChooseUser.Items.Add(user);
            }
        }

        private void btnCreateGroup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGroupName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm!");
                return;
            }

            if (checklb_ChooseUser.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một thành viên!");
                return;
            }

            GroupName = txtGroupName.Text;
            SelectedUsers = checklb_ChooseUser.CheckedItems.Cast<string>().ToList();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

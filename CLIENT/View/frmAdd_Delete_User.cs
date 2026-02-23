using COMMON.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CLIENT.View
{

    public enum GroupAction
    {
        Add,
        Delete
    }

    public partial class frmAdd_Delete_User : Form
    {

        private GroupAction _currentAction;
        private string _groupName;
        private ClientSocketConnect _client;


        public frmAdd_Delete_User()
        {
            InitializeComponent();
        }

        public frmAdd_Delete_User(List<string> usersToDisplay, GroupAction action, string groupName, ClientSocketConnect client) : this()
        {
            _currentAction = action;
            _groupName = groupName;
            _client = client;

            // Đưa danh sách user vào CheckedListBox
            checklb_ChooseUser.Items.Clear();
            checklb_ChooseUser.Items.AddRange(usersToDisplay.ToArray());

            // Đổi Text hiển thị dựa theo hành động
            if (_currentAction == GroupAction.Add)
            {
                this.Text = "Thêm thành viên";
                btnChoose.Text = "Thêm";
            }
            else if (_currentAction == GroupAction.Delete)
            {
                this.Text = "Xoá thành viên";
                btnChoose.Text = "Xoá";
            }
        }

        private void btnChoose_Click(object sender, EventArgs e)
        {
            // 1. Lấy danh sách các user được tích chọn
            List<string> selectedUsers = new List<string>();
            foreach (var item in checklb_ChooseUser.CheckedItems)
            {
                selectedUsers.Add(item.ToString());
            }

            if (selectedUsers.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 người dùng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Chuẩn bị dữ liệu gửi đi (Format: TênNhóm|User1,User2)
            string members = string.Join(",", selectedUsers);
            string rawData = $"{_groupName}|{members}";
            byte[] content = Encoding.UTF8.GetBytes(rawData);

            // 3. Xử lý dựa trên hành động Thêm hoặc Xóa
            if (_currentAction == GroupAction.Add)
            {
                DataPackage p = new DataPackage(PackageType.AddGroupMember, content);
                _client.Send(p.Pack());
                MessageBox.Show($"Đã thêm [{members}] vào nhóm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (_currentAction == GroupAction.Delete)
            {
                DataPackage p = new DataPackage(PackageType.RemoveGroupMember, content);
                _client.Send(p.Pack());
                MessageBox.Show($"Đã xoá [{members}] khỏi nhóm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Đóng form sau khi hoàn tất
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

using CLIENT.Process;
using COMMON.DTO;
using COMMON.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace CLIENT.View
{
    public partial class frmMain : Form
    {
        private ClientSocketConnect _client;

        private byte[] aesKey;

        // Lưu trữ IP của người dùng đang chat
        private string _selectedTargetIP = "";
        private Process.ChatLogic _chatLogic;

        public frmMain(ClientSocketConnect client)
        {
            InitializeComponent();

            _client = client;
            _client.OnRawDataReceived += (data) =>
            {
                HandleIncomingData(data);
            };

            _chatLogic = new Process.ChatLogic(_client);

            this.Load += FrmMain_Load;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            // Gửi yêu cầu cập nhật danh sách online ngay khi Form vừa hiện lên
            // Gửi một gói tin UserStatusUpdate với nội dung trống để Server biết và phản hồi
            DataPackage requestPackage = new DataPackage(PackageType.UserStatusUpdate, new byte[0]);
            _client.Send(requestPackage.Pack());
        }


        private void HandleIncomingData(byte[] data)
        {
            //Giải mã dữ liệu nhận được từ server
            DataPackage package = DataPackage.Unpack(data);

            // Danh sách người dùng đang online
            if (package.Type == PackageType.UserStatusUpdate)
            {
                string listUser = Encoding.UTF8.GetString(package.Content);
                string[] users = listUser.Split(',');

                this.Invoke(new Action(() =>
                {
                    lvOnlineUser.Items.Clear(); // Xóa sạch để cập nhật mới nhất
                    foreach (string user in users)
                    {
                        if (!string.IsNullOrEmpty(user))
                        {
                            ListViewItem item = new ListViewItem(user);
                            lvOnlineUser.Items.Add(item);
                        }
                    }
                }));
            }

            if (package.Type == PackageType.SecureMessage)
            {
                if (_client.AesKey != null)
                {
                    try
                    {
                        string decrypted = AES_Service.DecryptString(package.Content, _client.AesKey);

                        if (decrypted.Contains("|"))
                        {
                            string[] parts = decrypted.Split('|');
                            string targetIP = parts[0]; // Đây là IP người nhận (có thể là chính mình)
                            string message = parts[1]; // Nội dung tin nhắn

                            this.Invoke(new Action(() => {
                                // HIỂN THỊ TIN NHẮN:
                                txtChatBox.AppendText($"[{lbTargetName}]: {message}" + Environment.NewLine);
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        // Thêm log để debug xem lỗi giải mã hay lỗi tách chuỗi
                        MessageBox.Show("Lỗi nhận tin nhắn: " + ex.Message);
                    }
                }
            }
            else if (package.Type == PackageType.DH_KeyExchange)
            {
                this.aesKey = package.Content; // Lưu khóa này lại để dùng cho AES_Service
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show("Đã thiết lập kết nối bảo mật thành công!");
                }));
            }
        }



        private void btnCallVideo_Click(object sender, EventArgs e)
        {
            if (btnCallVideo.Enabled)
            {
                frmVideoCall videoCallForm = new frmVideoCall();
                videoCallForm.Show();
            }
        }

        private void btnSelectionChatBox_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(btnSelectionChatBox, new Point(0, btnSelectionChatBox.Height));
        }

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem đã nhận được key từ server chưa
            if (_client.AesKey == null)
            {
                MessageBox.Show("Chưa thiết lập kết nối bảo mật!", "Thiếu Key");
                return;
            }

            string message = txtChat.Text;
            if (!string.IsNullOrEmpty(message))
            {
                byte[] encryptedContent = COMMON.Security.AES_Service.EncryptString(message, _client.AesKey);

                // Đóng gói và gửi
                var package = new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.SecureMessage, encryptedContent);
                _client.Send(package.Pack());

                txtChatBox.AppendText("Tôi: " + message + Environment.NewLine);
                txtChat.Clear();
            }
        }

        private void btnChooseTarget_Click(object sender, EventArgs e)
        {
            if (lvOnlineUser.SelectedItems.Count > 0)
            {
                _selectedTargetIP = lvOnlineUser.SelectedItems[0].Text;
                lbTargetName.Text = "" + _selectedTargetIP;
                txtChatBox.Visible = true;
                MessageBox.Show($"Bắt đầu chat với [{_selectedTargetIP}]" + "Chatbox");
            }
        }
    }
}

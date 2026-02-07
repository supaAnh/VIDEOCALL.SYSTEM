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
                try
                {
                    string decrypted = AES_Service.DecryptString(package.Content, _client.AesKey);

                    if (decrypted.Contains("|"))
                    {
                        string[] parts = decrypted.Split('|');
                        string incomingSenderIP = parts[0];
                        string content = parts[1];

                        this.Invoke(new Action(() => {
                            // Chỉ hiển thị nếu người gửi chính là người mình đang chọn chat
                            if (incomingSenderIP == _selectedTargetIP)
                            {
                                txtChatBox.AppendText($"[{incomingSenderIP}]: {content}{Environment.NewLine}");
                            }
                            else
                            {
                                MessageBox.Show($"Tin nhắn mới từ [{incomingSenderIP}]: {content}", "Tin nhắn mới");
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi hiển thị chat: " + ex.Message);
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
            if (_client.AesKey == null)
            {
                MessageBox.Show("Chưa thiết lập kết nối bảo mật!", "Thiếu Key");
                return;
            }

            string message = txtChat.Text;
            // Kiểm tra xem đã chọn người để chat cùng chưa (_selectedTargetIP được gán khi nhấn nút "Trò chuyện")
            if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(_selectedTargetIP))
            {
                // ĐỊNH DẠNG: TargetIP|Nội dung tin nhắn
                string rawData = $"{_selectedTargetIP}|{message}";

                // Mã hóa toàn bộ chuỗi định dạng trên
                byte[] encryptedContent = AES_Service.EncryptString(rawData, _client.AesKey);

                // Đóng gói và gửi gói tin SecureMessage (Type 7)
                var package = new DataPackage(PackageType.SecureMessage, encryptedContent);
                _client.Send(package.Pack());

                // Hiển thị lên chính mình
                txtChatBox.AppendText("Tôi: " + message + Environment.NewLine);
                txtChat.Clear();
            }
            else if (string.IsNullOrEmpty(_selectedTargetIP))
            {
                MessageBox.Show("Vui lòng chọn một người trong danh sách để bắt đầu trò chuyện!");
            }
        }

        private void btnChooseTarget_Click(object sender, EventArgs e)
        {
            if (lvOnlineUser.SelectedItems.Count > 0)
            {
                _selectedTargetIP = lvOnlineUser.SelectedItems[0].Text;
                lbTargetName.Text = _selectedTargetIP;
                txtChatBox.Clear(); // Xóa trắng box chat cũ
                txtChatBox.Visible = true;

                // GỬI YÊU CẦU LẤY LỊCH SỬ CHO SERVER
                byte[] requestData = Encoding.UTF8.GetBytes(_selectedTargetIP);
                DataPackage p = new DataPackage(PackageType.RequestChatHistory, requestData);
                _client.Send(p.Pack());
            }
        }
    }
}

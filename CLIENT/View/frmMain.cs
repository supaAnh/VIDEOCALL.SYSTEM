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

        public frmMain(ClientSocketConnect client)
        {
            InitializeComponent();

            _client = client;
            _client.OnRawDataReceived += (data) => {
                HandleIncomingData(data);
            };
        }

        private void HandleIncomingData(byte[] data)
        {
            //Giải mã dữ liệu nhận được từ server
            DataPackage package = DataPackage.Unpack(data);

            if (package.Type == PackageType.SecureMessage)
            {
                // Sử dụng trực tiếp _client.AesKey
                if (_client.AesKey != null)
                {
                    string originalMessage = AES_Service.DecryptString(package.Content, _client.AesKey);
                    this.Invoke(new Action(() => {
                        txtChatBox.AppendText($"{lbTargetName}: " + originalMessage + Environment.NewLine);
                    }));
                }
            }
            else if (package.Type == PackageType.DH_KeyExchange)
            {
                this.aesKey = package.Content; // Lưu khóa này lại để dùng cho AES_Service
                this.Invoke(new Action(() => {
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
                MessageBox.Show("Chưa thiết lập kết nối bảo mật (thiếu Key)!");
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
    }
}

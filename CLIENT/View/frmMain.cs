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
        // Kết nối socket với server
        private ClientSocketConnect _client;

        // File logic xử lý gửi/nhận file
        private Logic.FileProcess _fileProcess;

        // Xử lý cuộc gọi video
        private Process.VideoCallProcess _videoCallLogic;

        private byte[] aesKey;

        // Lưu trữ IP của người dùng đang chat
        private string _selectedTargetIP = "";
        private Process.ChatLogic _chatLogic;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedMoniker { get; set; }

        public frmMain(ClientSocketConnect client)
        {
            InitializeComponent();

            // Lưu tham chiếu ClientSocketConnect
            _client = client;

            // Đăng ký sự kiện nhận dữ liệu từ server
            _client.OnRawDataReceived += (data) =>
            {
                HandleIncomingData(data);
            };

            // Khởi tạo ChatLogic
            _chatLogic = new Process.ChatLogic(_client);

            // Khởi tạo FileLogic
            _fileProcess = new Logic.FileProcess(_client);

            // Khởi tạo VideoCallProcess
            _videoCallLogic = new Process.VideoCallProcess(_client);

            // Đăng ký sự kiện Load của Form
            this.Load += FrmMain_Load;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            // Gửi yêu cầu cập nhật danh sách online ngay khi Form vừa hiện lên
            // Gửi một gói tin UserStatusUpdate với nội dung trống để Server biết và phản hồi
            DataPackage requestPackage = new DataPackage(PackageType.UserStatusUpdate, new byte[0]);
            _client.Send(requestPackage.Pack());
        }



        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Gửi gói tin thông báo ngắt kết nối đến server trước khi đóng form
            Application.Exit();
        }






        private void HandleIncomingData(byte[] data)
        {
            // Giải mã gói tin thô nhận được từ server
            DataPackage package = DataPackage.Unpack(data);

            switch (package.Type)
            {
                case PackageType.UserStatusUpdate:
                    HandleUserStatusUpdate(package.Content);
                    break;

                case PackageType.GroupUpdate:
                    HandleGroupUpdate(package.Content);
                    break;

                case PackageType.SendFile:
                    HandleIncomingFile(package.Content);
                    break;

                case PackageType.GroupMessage:
                    HandleGroupMessage(package.Content);
                    break;

                case PackageType.SecureMessage:
                    HandleSecureMessage(package.Content);
                    break;

                case PackageType.VideoCall:
                    HandleVideoCall(package.Content);
                    break;

                case PackageType.DH_KeyExchange:
                    this.aesKey = package.Content; // Lưu khóa AES dùng chung
                    this.Invoke(new Action(() => MessageBox.Show("Đã thiết lập kết nối bảo mật thành công!")));
                    break;

                default:
                    // Xử lý các gói tin không xác định nếu cần
                    break;
            }
        }

        // --- Tách các hàm xử lý con để code sạch sẽ hơn ---

        private void HandleUserStatusUpdate(byte[] content)
        {
            string listUser = Encoding.UTF8.GetString(content);
            string[] users = listUser.Split(',');

            this.Invoke(new Action(() =>
            {
                lvOnlineUser.Items.Clear();
                foreach (string user in users)
                {
                    if (!string.IsNullOrEmpty(user))
                    {
                        lvOnlineUser.Items.Add(new ListViewItem(user));
                    }
                }
            }));
        }

        private void HandleGroupUpdate(byte[] content)
        {
            string groupName = Encoding.UTF8.GetString(content);
            this.Invoke(new Action(() =>
            {
                ListViewItem groupItem = new ListViewItem(groupName);
                groupItem.ForeColor = Color.Blue;
                groupItem.Font = new Font(lvOnlineUser.Font, FontStyle.Bold);
                groupItem.Tag = "GROUP";
                lvOnlineUser.Items.Add(groupItem);
            }));
        }

        private void HandleIncomingFile(byte[] content)
        {
            this.Invoke(new Action(() =>
            {
                string fileName = _fileProcess.ProcessIncomingFile(content);
                if (!string.IsNullOrEmpty(fileName))
                {
                    txtChatBox.AppendText($"[Hệ thống]: Nhận file thành công: {fileName}{Environment.NewLine}");
                    MessageBox.Show($"Bạn đã nhận được file: {fileName}");
                }
            }));
        }

        private void HandleGroupMessage(byte[] content)
        {
            try
            {
                string decrypted = AES_Service.DecryptString(content, _client.AesKey);
                string[] parts = decrypted.Split('|');
                string gName = parts[0], sIP = parts[1], msg = parts[2];

                this.Invoke(new Action(() =>
                {
                    if (_selectedTargetIP == gName)
                        txtChatBox.AppendText($"[{sIP}]: {msg}{Environment.NewLine}");
                    else
                        MessageBox.Show($"Tin nhắn mới từ nhóm {gName}");
                }));
            }
            catch (Exception ex) { MessageBox.Show("Lỗi giải mã tin nhắn nhóm: " + ex.Message); }
        }

        private void HandleSecureMessage(byte[] content)
        {
            try
            {
                string decrypted = AES_Service.DecryptString(content, _client.AesKey);
                if (decrypted.Contains("|"))
                {
                    string[] parts = decrypted.Split('|');
                    string sIP = parts[0], msg = parts[1];

                    this.Invoke(new Action(() =>
                    {
                        if (sIP == _selectedTargetIP)
                            txtChatBox.AppendText($"[{sIP}]: {msg}{Environment.NewLine}");
                        else
                            MessageBox.Show($"Tin nhắn mới từ [{sIP}]: {msg}");
                    }));
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi hiển thị chat: " + ex.Message); }
        }

        private void HandleVideoCall(byte[] content)
        {
            string rawSignal = Encoding.UTF8.GetString(content);
            string[] parts = rawSignal.Split('|');

            // Cấu trúc nhận được: [SenderIP]|[Status]|[Payload]
            if (parts.Length < 2) return;

            string senderIP = parts[0];
            string status = parts[1];
            string payload = parts.Length >= 3 ? parts[2] : "";

            this.Invoke(new Action(() =>
            {
                switch (status)
                {
                    case "Request":
                        var res = MessageBox.Show($"Cuộc gọi video từ {senderIP}. Đồng ý?",
                                  "Video Call", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (res == DialogResult.Yes)
                        {
                            using (frmSelectOption sd = new frmSelectOption())
                            {
                                if (sd.ShowDialog() == DialogResult.OK)
                                {
                                    // Gửi tín hiệu chấp nhận
                                    _videoCallLogic.SendVideoCallSignal(senderIP, "Accept");

                                    // TRUYỀN THÊM sd.IsCameraOn, sd.IsMicOn
                                    frmVideoCall activeCallForm = new frmVideoCall(
                                        senderIP,
                                        sd.SelectedMoniker,
                                        _videoCallLogic,
                                        sd.IsCameraOn,
                                        sd.IsMicOn
                                    );

                                    activeCallForm.Show();
                                }
                                else { _videoCallLogic.SendVideoCallSignal(senderIP, "Refuse"); }
                            }
                        }
                        break;

                    case "Frame":
                    case "Audio":
                        if (!string.IsNullOrEmpty(payload))
                        {
                            _videoCallLogic.ProcessIncomingVideoData(senderIP, status, payload);
                        }
                        break;

                    case "Accept":
                        var callForm = Application.OpenForms.OfType<frmVideoCall>().FirstOrDefault();
                        if (callForm != null)
                        {
                            // Chỉ thêm người dùng vào giao diện
                            callForm.AddParticipant(senderIP);

                        }
                        break;

                    case "Refuse":
                        MessageBox.Show($"[{senderIP}] từ chối cuộc gọi.");
                        break;
                }
            }));
        }



        private void btnCallVideo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedTargetIP)) return;

            using (frmSelectOption selectDevice = new frmSelectOption())
            {
                if (selectDevice.ShowDialog() == DialogResult.OK)
                {
                    // TRUYỀN THÊM selectDevice.IsCameraOn, selectDevice.IsMicOn
                    frmVideoCall callForm = new frmVideoCall(
                        _selectedTargetIP,
                        selectDevice.SelectedMoniker,
                        _videoCallLogic,
                        selectDevice.IsCameraOn,
                        selectDevice.IsMicOn
                    );

                    callForm.Show();

                    // Gửi tín hiệu yêu cầu gọi
                    _videoCallLogic.SendSignal(_selectedTargetIP, "Request");
                }
            }
        }



        private void btnSelectionChatBox_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(btnSelectionChatBox, new Point(0, btnSelectionChatBox.Height));
        }

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedTargetIP) || string.IsNullOrEmpty(txtChat.Text)) return;

            string message = txtChat.Text;

            // Kiểm tra xem mục tiêu đang chọn là Group hay User thông qua Tag
            bool isGroup = lvOnlineUser.SelectedItems.Count > 0 && lvOnlineUser.SelectedItems[0].Tag?.ToString() == "GROUP";

            if (isGroup)
            {
                // Gửi tin nhắn nhóm
                string rawData = $"{_selectedTargetIP}|{message}";
                byte[] encrypted = AES_Service.EncryptString(rawData, _client.AesKey);
                var package = new DataPackage(PackageType.GroupMessage, encrypted);
                _client.Send(package.Pack());
            }
            else
            {
                // Gửi tin nhắn riêng (Logic cũ của bạn)
                string rawData = $"{_selectedTargetIP}|{message}";
                byte[] encrypted = AES_Service.EncryptString(rawData, _client.AesKey);
                var package = new DataPackage(PackageType.SecureMessage, encrypted);
                _client.Send(package.Pack());
            }

            txtChatBox.AppendText($"Tôi: {message}{Environment.NewLine}");
            txtChat.Clear();
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

        private void btnCreateGroupChat_Click(object sender, EventArgs e)
        {
            // Lấy danh sách IP từ lvOnlineUser hiện tại
            List<string> users = new List<string>();
            foreach (ListViewItem item in lvOnlineUser.Items)
            {
                // Chỉ lấy các User, bỏ qua các Group đã có trong danh sách
                if (item.Tag?.ToString() != "GROUP")
                {
                    users.Add(item.Text);
                }
            }

            using (var frm = new frmCreateGroupChat(users))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    // Client chỉ gửi danh sách những người được chọn
                    // Server sẽ tự động thêm người tạo vào
                    string members = string.Join(",", frm.SelectedUsers);
                    string rawData = $"{frm.GroupName}|{members}";

                    byte[] content = Encoding.UTF8.GetBytes(rawData);
                    DataPackage p = new DataPackage(PackageType.CreateGroup, content);
                    _client.Send(p.Pack());
                }
            }
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _fileProcess.ExecuteSendFile(_selectedTargetIP, openFileDialog1.FileName);
                txtChatBox.AppendText($"[Hệ thống]: Đã gửi file [{Path.GetFileName(openFileDialog1.FileName)}] thành công!{Environment.NewLine}");
            }
        }
    }
}

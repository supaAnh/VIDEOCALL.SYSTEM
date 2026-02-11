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


        private void HandleIncomingData(byte[] data)
        {
            //Giải mã dữ liệu nhận được từ server
            DataPackage package = DataPackage.Unpack(data);

            //
            // Danh sách người dùng đang online
            //
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

            //
            // Cập nhật nhóm mới
            //
            else if (package.Type == PackageType.GroupUpdate)
            {
                string groupName = Encoding.UTF8.GetString(package.Content);
                this.Invoke(new Action(() =>
                {
                    ListViewItem groupItem = new ListViewItem(groupName);
                    groupItem.ForeColor = Color.Blue; // Đổi màu xanh để nhận diện là Nhóm
                    groupItem.Font = new Font(lvOnlineUser.Font, FontStyle.Bold);
                    groupItem.Tag = "GROUP"; // Đánh dấu loại để xử lý logic gửi tin sau này
                    lvOnlineUser.Items.Add(groupItem);
                }));
            }

            //
            // Gửi file
            //
            else if (package.Type == PackageType.SendFile)
            {
                this.Invoke(new Action(() =>
                {
                    // Gọi hàm xử lý và nhận tên file trả về
                    string fileName = _fileProcess.ProcessIncomingFile(package.Content);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        // Hiển thị thông báo lên khung chat
                        txtChatBox.AppendText($"[Hệ thống]: Đã nhận file thành công: {fileName}{Environment.NewLine}");
                        MessageBox.Show($"Bạn đã nhận được file: {fileName}", "Thông báo");
                    }
                    else
                    {
                        txtChatBox.AppendText($"[Hệ thống]: Nhận file thất bại (Lỗi giải mã).{Environment.NewLine}");
                    }
                }));
            }
            //
            // Tin nhắn nhóm
            //
            else if (package.Type == PackageType.GroupMessage)
            {
                try
                {
                    string decrypted = AES_Service.DecryptString(package.Content, _client.AesKey);
                    string[] parts = decrypted.Split('|');
                    string groupName = parts[0];
                    string senderIP = parts[1];
                    string content = parts[2];

                    this.Invoke(new Action(() =>
                    {
                        // Hiển thị nếu đang mở đúng cửa sổ chat của nhóm đó
                        if (_selectedTargetIP == groupName)
                        {
                            txtChatBox.AppendText($"[{senderIP}]: {content}{Environment.NewLine}");
                        }
                        else
                        {
                            // Thông báo tin nhắn mới từ nhóm khác
                            MessageBox.Show($"Tin nhắn mới từ nhóm {groupName}");
                        }
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi giải mã tin nhắn nhóm: " + ex.Message);
                }
            }

            //
            // Tin nhắn bảo mật từ người khác
            //
            else if (package.Type == PackageType.SecureMessage)
            {
                try
                {
                    string decrypted = AES_Service.DecryptString(package.Content, _client.AesKey);

                    if (decrypted.Contains("|"))
                    {
                        string[] parts = decrypted.Split('|');
                        string incomingSenderIP = parts[0];
                        string content = parts[1];

                        this.Invoke(new Action(() =>
                        {
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

            //
            // Cuộc gọi video
            //
            else if (package.Type == PackageType.VideoCall)
            {
                string rawSignal = Encoding.UTF8.GetString(package.Content);
                string[] parts = rawSignal.Split('|');
                string senderIP = parts[0];
                string status = parts[1];

                this.Invoke(new Action(() => {
                    switch (status)
                    {
                        case "Request":
                            var res = MessageBox.Show($"Cuộc gọi từ {senderIP}. Đồng ý?", "Video Call", MessageBoxButtons.YesNo);
                            if (res == DialogResult.Yes)
                            {
                                // 1. Sửa tên Form từ frmDeviceSelection thành frmSelectOption
                                using (frmSelectOption sd = new frmSelectOption())
                                {
                                    if (sd.ShowDialog() == DialogResult.OK)
                                    {
                                        _videoCallLogic.SendVideoCallSignal(senderIP, "Accept");

                                        // 2. Sửa lỗi biến 'selectDevice' không tồn tại thành 'sd' (tên biến instance bạn vừa tạo)
                                        // 3. Sử dụng 'senderIP' thay vì '_selectedTargetIP' vì đây là người đang gọi đến bạn
                                        frmVideoCall callForm = new frmVideoCall(senderIP, sd.SelectedMoniker, _videoCallLogic);
                                        callForm.Show();
                                    }
                                }
                            }
                            else
                            {
                                _videoCallLogic.SendVideoCallSignal(senderIP, "Refuse");
                            }
                            break;

                        case "Accept":
                            // Logic khi đối phương đồng ý: Bạn cần mở Form Video Call tại đây
                            // Lưu ý: Cần lấy được Moniker mà bạn đã chọn từ lúc gửi Request
                            MessageBox.Show("Đối phương đã chấp nhận cuộc gọi.");
                            break;

                        case "Refuse":
                            MessageBox.Show("Đối phương đã từ chối cuộc gọi.");
                            // Thêm logic để tìm và đóng frmVideoCall đang ở trạng thái chờ (Ringing) nếu có
                            break;
                    }
                }));
            }

            //
            // Thiết lập kết nối bảo mật (nhận khóa DH từ server)
            //
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
            if (string.IsNullOrEmpty(_selectedTargetIP)) return;

            using (frmSelectOption selectDevice = new frmSelectOption())
            {
                if (selectDevice.ShowDialog() == DialogResult.OK)
                {
                    // Truyền IP mục tiêu, Moniker thiết bị và tham chiếu logic vào Form
                    frmVideoCall callForm = new frmVideoCall(_selectedTargetIP, selectDevice.SelectedMoniker, _videoCallLogic);
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

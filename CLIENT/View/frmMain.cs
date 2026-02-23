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


        private Dictionary<string, List<string>> _groupMembersDict = new Dictionary<string, List<string>>();


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
            // Gửi gói tin thông báo đăng xuất cho Server trước khi đóng
            Environment.Exit(0);
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

                case PackageType.Notification:
                    HandleNotification(package.Content);
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

        // Hàm xử lý khi nhận được thông báo từ Server
        private void HandleNotification(byte[] content)
        {
            // Do ở phía Server chúng ta gửi plain text (không mã hóa AES gói Notification)
            // nên ở đây chỉ cần dịch byte thẳng ra chuỗi UTF8
            string message = Encoding.UTF8.GetString(content);

            // Chạy trên luồng UI để hiển thị MessageBox an toàn
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(message, "Thông báo từ Server", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Đóng toàn bộ chương trình ngay lập tức
                Application.Exit();
            }));
        }

        // Hàm xử lý khi nhận được cập nhật về nhóm (tên nhóm + danh sách thành viên)
        private void HandleGroupUpdate(byte[] content)
        {
            string payload = Encoding.UTF8.GetString(content);
            // Tách dữ liệu: parts[0] là Tên nhóm, parts[1] là danh sách user
            string[] parts = payload.Split('|');

            if (parts.Length < 1) return;

            string groupName = parts[0];

            // Lưu/Cập nhật danh sách thành viên vào Dictionary của Client
            List<string> members = new List<string>();
            if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[1]))
            {
                members = parts[1].Split(',').ToList();
            }

            // Lấy Username của chính mình từ Tiêu đề Form
            string myUsername = this.Text.Replace("CLIENT - ", "").Trim();

            this.Invoke(new Action(() =>
            {
                // Nếu Client KHÔNG CÒN nằm trong danh sách thành viên
                if (!members.Contains(myUsername))
                {
                    // 1. Xóa khỏi Dictionary lưu trữ của Client
                    if (_groupMembersDict.ContainsKey(groupName))
                    {
                        _groupMembersDict.Remove(groupName);
                    }

                    // 2. Xóa khỏi ListView hiển thị
                    foreach (ListViewItem item in lvOnlineUser.Items)
                    {
                        if (item.Text == groupName && item.Tag?.ToString() == "GROUP")
                        {
                            lvOnlineUser.Items.Remove(item);
                            break;
                        }
                    }

                    // 3. Nếu Client đang mở chính box chat của nhóm này thì reset lại
                    if (_selectedTargetIP == groupName)
                    {
                        _selectedTargetIP = "";
                        lbTargetName.Text = "Chưa chọn";
                        txtChatBox.Clear();
                        txtChatBox.Visible = false;
                    }

                    // 4. Hiển thị thông báo bị xoá
                    MessageBox.Show($"Bạn đã bị xoá khỏi nhóm {groupName}.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Nếu Client VẪN CÒN trong nhóm (Được thêm vào hoặc nhóm mới tạo)
                    _groupMembersDict[groupName] = members;

                    // Kiểm tra xem nhóm đã có trên ListView chưa, tránh Add trùng lặp khi Update
                    bool exists = false;
                    foreach (ListViewItem item in lvOnlineUser.Items)
                    {
                        if (item.Text == groupName && item.Tag?.ToString() == "GROUP")
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        ListViewItem groupItem = new ListViewItem(groupName);
                        groupItem.ForeColor = Color.Blue;
                        groupItem.Font = new Font(lvOnlineUser.Font, FontStyle.Bold);
                        groupItem.Tag = "GROUP";
                        lvOnlineUser.Items.Add(groupItem);
                    }
                }
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
                string decrypted = COMMON.Security.AES_Service.DecryptString(content, _client.AesKey);
                string[] parts = decrypted.Split('|');
                string gName = parts[0], sIP = parts[1], msg = parts[2];

                this.Invoke(new Action(() =>
                {
                    if (_selectedTargetIP == gName)
                    {
                        string myUsername = this.Text.Replace("CLIENT - ", "").Trim();
                        string displayUser = (sIP == myUsername) ? "Tôi" : sIP;
                        txtChatBox.AppendText($"[{displayUser}]: {msg}{Environment.NewLine}");
                    }
                    else
                    {
                        MessageBox.Show($"Tin nhắn mới từ nhóm {gName}");
                    }
                }));
            }
            catch (Exception ex) { MessageBox.Show("Lỗi giải mã tin nhắn nhóm: " + ex.Message); }
        }

        private void HandleSecureMessage(byte[] content)
        {
            try
            {
                string decrypted = COMMON.Security.AES_Service.DecryptString(content, _client.AesKey);
                if (decrypted.Contains("|"))
                {
                    string[] parts = decrypted.Split('|');
                    string sIP = parts[0], msg = parts[1];

                    this.Invoke(new Action(() =>
                    {
                        // Lấy Username của chính mình từ Tiêu đề Form
                        string myUsername = this.Text.Replace("CLIENT - ", "").Trim();

                        // Cập nhật khung chat mới nhất: Nếu tin nhắn do chính mình gửi hoặc người đang chat gửi
                        if (sIP == _selectedTargetIP || sIP == myUsername)
                        {
                            string displayUser = (sIP == myUsername) ? "Tôi" : sIP;
                            txtChatBox.AppendText($"[{displayUser}]: {msg}{Environment.NewLine}");
                        }
                        else
                        {
                            MessageBox.Show($"Tin nhắn mới từ [{sIP}]: {msg}");
                        }
                    }));
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi hiển thị chat: " + ex.Message); }
        }



        private void HandleVideoCall(byte[] content)
        {
            // Giải mã nội dung gói tin trước khi đọc bằng khóa AES
            string rawSignal = COMMON.Security.AES_Service.DecryptString(content, _client.AesKey);
            string[] parts = rawSignal.Split('|');

            // Cấu trúc nhận được có thể là:
            // Gọi 1-1: [SenderIP]|[Status]|[Payload]
            // Gọi Nhóm: [TênNhóm]|[Status]|[ActualSender]|[Payload]
            if (parts.Length < 2) return;

            string targetOrSender = parts[0];
            string status = parts[1];

            // Mặc định cho 1-1
            string actualSender = targetOrSender;
            string payload = parts.Length >= 3 ? parts[2] : "";

            // Nếu là gọi Nhóm (4 phần), ghi đè lại actualSender
            if (parts.Length >= 4)
            {
                actualSender = parts[2];
                payload = parts[3];
            }

            this.Invoke(new Action(() =>
            {
                // Tìm xem có form Video Call nào đang mở không
                var activeCallForm = Application.OpenForms.OfType<frmVideoCall>().FirstOrDefault();

                switch (status)
                {
                    case "Request":
                        // TÍNH NĂNG JOIN GIỮA CHỪNG
                        if (activeCallForm != null)
                        {
                            // Nếu đang trong cuộc gọi, tự động gửi Accept cho người mới và thêm vào giao diện
                            _videoCallLogic.SendVideoCallSignal(actualSender, "Accept");
                            activeCallForm.AddParticipant(actualSender);
                        }
                        else
                        {
                            // Nếu chưa có cuộc gọi, hiển thị hộp thoại xác nhận bình thường
                            string displayCaller = (targetOrSender != actualSender) ? $"{targetOrSender} (từ {actualSender})" : actualSender;
                            var res = MessageBox.Show($"Cuộc gọi video từ {displayCaller}. Đồng ý?",
                                      "Video Call", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (res == DialogResult.Yes)
                            {
                                using (frmSelectOption sd = new frmSelectOption())
                                {
                                    if (sd.ShowDialog() == DialogResult.OK)
                                    {
                                        // Gửi tín hiệu chấp nhận 1-1 trực tiếp tới người gọi
                                        _videoCallLogic.SendVideoCallSignal(actualSender, "Accept");

                                        // Mở form video call, targetOrSender giữ nguyên để biết là gửi frame vào Group hay User
                                        frmVideoCall callForm = new frmVideoCall(
                                            targetOrSender,
                                            sd.SelectedMoniker,
                                            _videoCallLogic,
                                            sd.IsCameraOn,
                                            sd.IsMicOn
                                        );

                                        // Thêm định danh THẬT của người gọi vào UI
                                        callForm.AddParticipant(actualSender);
                                        callForm.Show();
                                    }
                                    else { _videoCallLogic.SendVideoCallSignal(actualSender, "Refuse"); }
                                }
                            }
                            else
                            {
                                _videoCallLogic.SendVideoCallSignal(actualSender, "Refuse");
                            }
                        }
                        break;

                    case "Frame":
                    case "Audio":
                        // Bỏ qua do luồng dữ liệu truyền Media được xử lý bên trong VideoCallProcess
                        break;

                    case "Accept":
                        if (activeCallForm != null)
                        {
                            // Dùng actualSender thay vì Tên Nhóm
                            activeCallForm.AddParticipant(actualSender);
                        }
                        break;

                    case "Refuse":
                        // Chỉ hiển thị thông báo từ chối nếu gọi 1-1 để tránh phiền trong Group
                        if (targetOrSender == actualSender)
                        {
                            MessageBox.Show($"[{actualSender}] từ chối cuộc gọi.");
                        }
                        break;

                    case "RecordStart":
                        MessageBox.Show($"[{actualSender}] đang ghi hình cuộc gọi này!", "Thông báo bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            // Kiểm tra xem người dùng có đang chọn một mục trong danh sách và mục đó có phải là Group không
            bool isGroup = lvOnlineUser.SelectedItems.Count > 0 && lvOnlineUser.SelectedItems[0].Tag?.ToString() == "GROUP";

            if (isGroup)
            {
                // Nếu là Group thì hiển thị Context Menu
                contextMenuStrip1.Show(btnSelectionChatBox, new Point(0, btnSelectionChatBox.Height));
            }
            else
            {
                // Nếu chat 1-1 thì vô hiệu hóa (hoặc hiện thông báo)
                MessageBox.Show("Chức năng này chỉ khả dụng khi đang chat trong Nhóm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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

        private void btnWatchRecord_Click(object sender, EventArgs e)
        {
            frmRecord recordForm = new frmRecord();
            recordForm.Show();
        }




        // Hàm lấy danh sách thành viên hiện tại của một nhóm
        private List<string> GetCurrentGroupMembers(string groupName)
        {
            // Nếu Client đã lưu thông tin nhóm này, trả về bản sao danh sách của nhóm đó
            if (_groupMembersDict.ContainsKey(groupName))
            {
                return new List<string>(_groupMembersDict[groupName]);
            }

            // Nếu chưa có trả về rỗng
            return new List<string>();
        }

        private void thêmThànhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> allOnlineUsers = new List<string>();
            foreach (ListViewItem item in lvOnlineUser.Items)
            {
                if (item.Tag?.ToString() != "GROUP") allOnlineUsers.Add(item.Text);
            }

            List<string> currentMembers = GetCurrentGroupMembers(_selectedTargetIP);
            List<string> usersCanBeAdded = new List<string>();

            // Lọc ra các user CHƯA có trong nhóm
            foreach (string user in allOnlineUsers)
            {
                if (!currentMembers.Contains(user)) usersCanBeAdded.Add(user);
            }

            if (usersCanBeAdded.Count == 0)
            {
                MessageBox.Show("Tất cả những người đang online đều đã có trong nhóm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Truyền cờ GroupAction.Add
            using (var frm = new frmAdd_Delete_User(usersCanBeAdded, GroupAction.Add, _selectedTargetIP, _client))
            {
                frm.ShowDialog();
            }
        }

        private void xoáThànhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> currentMembers = GetCurrentGroupMembers(_selectedTargetIP);
            string myUsername = this.Text.Replace("CLIENT - ", "").Trim();
            currentMembers.Remove(myUsername); // Không cho tự xoá chính mình

            if (currentMembers.Count == 0)
            {
                MessageBox.Show("Nhóm không có thành viên nào khác để xoá!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Truyền cờ GroupAction.Delete
            using (var frm = new frmAdd_Delete_User(currentMembers, GroupAction.Delete, _selectedTargetIP, _client))
            {
                frm.ShowDialog();
            }
        }

        private void btnSignOut_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất tài khoản hiện tại?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Khởi động lại ứng dụng 
                System.Diagnostics.Process.Start(Application.ExecutablePath);

                // Đảm bảo thoát hoàn toàn phiên bản cũ để tránh rò rỉ tài nguyên hoặc kết nối
                Environment.Exit(0);
            }

        }
    }
}

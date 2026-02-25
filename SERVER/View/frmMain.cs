using SERVER.LogUI;
using SERVER.View;

namespace SERVER
{
    public partial class frmMain : Form
    {
        // Khai báo biến _server ở cấp độ Class để cả nút Mở và Tắt đều dùng chung được
        private SocketConnect _server;

        public frmMain()
        {
            InitializeComponent();
            LogViewUI.Initialize(this.listViewHistory, this.listViewClientConnected);
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            btnExit.Enabled = false;

            // Khi Form chính được tải lên, tự động gọi hàm LoadSessionComboBox để điền dữ liệu vào ComboBox
            LoadSessionComboBox();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (_server != null)
            {
                // Gọi hàm tắt Server
                _server.StopServer();
                _server = null;

                // Cập nhật lại giao diện
                btnConnect.Enabled = true;   // Cho phép Mở lại
                btnExit.Enabled = false;     // Vô hiệu hóa nút Tắt
                textNumPort.Enabled = true;  // Cho phép nhập Port mới

                // Xóa sạch danh sách Client đang hiển thị trên giao diện
                listViewClientConnected.Items.Clear();

                LogViewUI.AddLog("Đã ngắt kết nối Server.");
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                _server = new SocketConnect();
                int port = int.Parse(textNumPort.Text);
                _server.StartServer(port);
                LogViewUI.AddLog($"Đã khởi động Server ở port: {port}");

                // Cập nhật trạng thái nút bấm sau khi mở thành công
                btnConnect.Enabled = false;  // Vô hiệu hóa nút Mở
                btnExit.Enabled = true;      // Mở khóa nút Tắt
                textNumPort.Enabled = false; // Khóa luôn ô đổi Port để tránh đổi nhầm khi đang chạy
            }
            catch (Exception ex)
            {
                LogViewUI.AddLog("Lỗi khi mở Server: " + ex.Message);
                _server = null;
            }
        }

        private void btnWatchRecord_Click(object sender, EventArgs e)
        {
            frmRecordManage recordManage = new frmRecordManage();
            recordManage.Show();
        }

        private void btnClientOut_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem Server đã được khởi động chưa
            if (_server == null)
            {
                MessageBox.Show("Server chưa được mở!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra xem người dùng đã chọn Client nào trong danh sách (ListView) chưa
            if (listViewClientConnected.SelectedItems.Count > 0)
            {
                // Lấy Username (hoặc IP) của Client đang được chọn ở cột đầu tiên (Text)
                string selectedClient = listViewClientConnected.SelectedItems[0].Text;

                // Xác nhận lại để tránh lỡ tay bấm nhầm
                DialogResult dialogResult = MessageBox.Show(
                    $"Bạn có chắc chắn muốn ngắt kết nối Client '{selectedClient}' không?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    // Gọi hàm ngắt kết nối bên class SocketConnect
                    _server.DisconnectClient(selectedClient);

                    // Cập nhật lại giao diện: Xóa Client đó khỏi danh sách hiển thị
                    listViewClientConnected.Items.Remove(listViewClientConnected.SelectedItems[0]);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một Client trong danh sách để ngắt kết nối!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        // gọi mỗi khi mở Form hoặc sau khi đóng cửa sổ quản lý phiên làm việc để cập nhật lại danh sách phiên làm việc hiện có
        private void LoadSessionComboBox()
        {
            comboBoxIDSessionLog.Items.Clear();
            comboBoxIDSessionLog.Items.Add("Current Session"); // Mặc định ở dòng đầu tiên

            SERVER.Database.DatabaseConnect db = new SERVER.Database.DatabaseConnect();
            var sessions = db.GetSessionList();

            foreach (var s in sessions)
            {
                comboBoxIDSessionLog.Items.Add(s.Value); // Value ở đây có dạng "Thời_gian - SessionID"
            }

            // Tự động chọn dòng đầu tiên
            comboBoxIDSessionLog.SelectedIndex = 0;
        }

        private void comboBoxIDSessionLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            SERVER.Database.DatabaseConnect db = new SERVER.Database.DatabaseConnect();

            if (comboBoxIDSessionLog.SelectedIndex == 0)
            {
                // Người dùng chọn xem Hiện tại
                LogViewUI.IsViewingCurrentSession = true;

                // Load lại toàn bộ log của session hiện hành từ DB ra ListView
                if (!string.IsNullOrEmpty(LogViewUI.CurrentSessionID))
                {
                    var logs = db.GetLogsBySession(LogViewUI.CurrentSessionID);
                    LogViewUI.LoadHistoryToView(logs);
                }
                else
                {
                    listViewHistory.Items.Clear();
                }
            }
            else
            {
                // Người dùng chọn xem Quá khứ
                LogViewUI.IsViewingCurrentSession = false;

                string selectedText = comboBoxIDSessionLog.SelectedItem.ToString();

                // Tách chuỗi theo định dạng "Thời_gian - SessionID" để lấy SessionID
                string[] parts = selectedText.Split(new string[] { " - " }, StringSplitOptions.None);

                if (parts.Length == 2)
                {
                    string sessionId = parts[1];
                    var logs = db.GetLogsBySession(sessionId);
                    LogViewUI.LoadHistoryToView(logs);
                }
            }
        }
    }
}

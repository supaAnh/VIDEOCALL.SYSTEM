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
    }
}

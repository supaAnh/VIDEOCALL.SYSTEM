using SERVER.LogUI;

namespace SERVER
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            SERVER.LogUI.LogViewUI.Initialize(this.listViewHistory);
        }
        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                SocketConnect server = new SocketConnect();
                int port = int.Parse(textNumPort.Text);
                server.StartServer(port);
                LogViewUI.AddLog($"Đã khởi động Server ở port: {port}");
            }
            catch (Exception ex)
            {
                LogViewUI.AddLog("Lỗi khi mở Server: " + ex.Message);
            }
        }

        
    }
}

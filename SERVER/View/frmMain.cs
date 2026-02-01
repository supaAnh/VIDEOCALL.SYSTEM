namespace SERVER
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
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
                MessageBox.Show($"Đã khởi động Server ở port: {port}", "Server đã chạy");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở Server: " + ex.Message);
            }
        }

        
    }
}

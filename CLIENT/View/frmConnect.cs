using CLIENT.View;

namespace CLIENT.View
{
    public partial class frmConnect : Form
    {
        public frmConnect()
        {
            InitializeComponent();
        }

        private void frmConnect_Load(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                while (true)
                {
                    ClientSocketConnect client = new ClientSocketConnect();
                    int port = int.Parse(textNumPort.Text);
                    client.Connect(port);
                    this.Hide();

                    frmLogin loginForm = new frmLogin();
                    loginForm.Show();
                    break;
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Hãy kiểm tra port!", "Port lỗi:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi kết nối!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
    }
}

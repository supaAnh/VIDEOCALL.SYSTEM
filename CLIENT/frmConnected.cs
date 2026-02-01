using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CLIENT.View;
namespace CLIENT
{
    public partial class frmConnected : Form
    {
        public frmConnected()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ClientSocketConnect client = new ClientSocketConnect();
                int port = int.Parse(textNumPort.Text);
                client.Connect(port);
                this.Hide();

                frmLogin loginForm = new frmLogin();
                loginForm.Show();
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

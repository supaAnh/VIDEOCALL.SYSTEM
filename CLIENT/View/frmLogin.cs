using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CLIENT.View
{
    public partial class frmLogin : Form
    {
        private ClientSocketConnect _client;
        public frmLogin(ClientSocketConnect client)
        {
            InitializeComponent();
            _client = client;
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                txtPassword.UseSystemPasswordChar = false;
            }
            else
            {
                txtPassword.UseSystemPasswordChar = true;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Xử lý đăng nhập
            btnLogin.Enabled = true;
            frmMain mainForm = new frmMain(_client);
            mainForm.Show();
            this.Hide();
        }
    }
}

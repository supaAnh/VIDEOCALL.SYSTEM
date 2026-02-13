using COMMON.DTO;
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
        private bool _isWaitLogin = false;


        private ClientSocketConnect _client;
        public frmLogin(ClientSocketConnect client)
        {
            InitializeComponent();
            _client = client;

            // Đăng ký sự kiện nhận dữ liệu từ Server
            _client.OnRawDataReceived += Client_OnDataReceived;

        }

        private void frmLogin_Load(object sender, EventArgs e)
        {

        }

        // Xử lý dữ liệu nhận được từ Server
        private void Client_OnDataReceived(byte[] data)
        {
            try
            {
                var pkg = DataPackage.Unpack(data);

                // Xử lý Login Response
                if (pkg.Type == PackageType.LoginResponse)
                {
                    string res = COMMON.Security.AES_Service.DecryptString(pkg.Content, _client.AesKey);

                    this.Invoke(new Action(() =>
                    {
                        if (res == "OK")
                        {
                            // Đăng nhập thành công
                            _client.OnRawDataReceived -= Client_OnDataReceived;
                            frmMain mainForm = new frmMain(_client);
                            mainForm.Text = $"CLIENT - {txtUsername.Text}";
                            mainForm.Show();
                            this.Hide();
                        }
                        else if (res == "DUPLICATE")
                        {
                            // TRƯỜNG HỢP: Tài khoản đang được sử dụng
                            MessageBox.Show("Tài khoản này đang được đăng nhập ở nơi khác!\nVui lòng kiểm tra lại.", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            btnLogin.Enabled = true;
                            lbStatus.Text = "Tài khoản đang online.";
                            _isWaitLogin = false;
                        }
                        else
                        {
                            // TRƯỜNG HỢP: Sai User/Pass
                            MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            btnLogin.Enabled = true;
                            lbStatus.Text = "Đăng nhập thất bại.";
                            _isWaitLogin = false;
                        }
                    }));
                }

                // Xử lý Register Response
                if (pkg.Type == PackageType.Register)
                {
                    string res = COMMON.Security.AES_Service.DecryptString(pkg.Content, _client.AesKey);
                    this.Invoke(new Action(() =>
                    {
                        if (res == "OK") MessageBox.Show("Đăng ký thành công! Hãy đăng nhập.");
                        else MessageBox.Show("Đăng ký thất bại (Username đã tồn tại).");

                        lbStatus.Text = "Sẵn sàng.";
                    }));
                }
            }
            catch { }
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
            if (_client.AesKey == null)
            {
                MessageBox.Show("Chưa nhận được khóa bảo mật từ Server!");
                return;
            }

            string u = txtUsername.Text.Trim();
            string p = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p)) return;

            // Gửi gói tin Authenticate: Username|Password
            string raw = $"{u}|{p}";
            byte[] enc = COMMON.Security.AES_Service.EncryptString(raw, _client.AesKey);

            _client.Send(new DataPackage(PackageType.Authenticate, enc).Pack());

            btnLogin.Enabled = false; // Chặn click liên tục
            _isWaitLogin = true;
            lbStatus.Text = "Đang xác thực...";
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (_client.AesKey == null) return;

            string u = txtUsername.Text.Trim();
            string p = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            string raw = $"{u}|{p}";
            byte[] enc = COMMON.Security.AES_Service.EncryptString(raw, _client.AesKey);

            _client.Send(new DataPackage(PackageType.Register, enc).Pack());
            lbStatus.Text = "Đang đăng ký...";
        }
    }
}

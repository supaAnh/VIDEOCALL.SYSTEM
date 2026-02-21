using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace CLIENT.View
{
    public partial class frmRecord : Form
    {
        private string _downloadsPath;

        public frmRecord()
        {
            InitializeComponent();

            // Lấy chính xác đường dẫn thư mục Downloads
            _downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            // Gọi trực tiếp hàm lấy danh sách ngay khi Form vừa khởi tạo xong (không chờ sự kiện Load)
            LoadRecordList();

            // Gán sự kiện cho các nút bấm (nếu bạn chưa gán bằng giao diện Designer)
            btnReturn.Click += BtnReturn_Click;
            btnChoose.Click += BtnChoose_Click;
        }

        private void LoadRecordList()
        {
            lbRecordList.Items.Clear();

            try
            {
                // Kiểm tra xem thư mục có tồn tại không
                if (Directory.Exists(_downloadsPath))
                {
                    // Lấy tất cả các file có đuôi .mp4
                    string[] files = Directory.GetFiles(_downloadsPath, "*.mp4");

                    if (files.Length > 0)
                    {
                        foreach (string file in files)
                        {
                            // Chỉ thêm tên file vào danh sách để dễ nhìn
                            lbRecordList.Items.Add(Path.GetFileName(file));
                        }
                    }
                    else
                    {
                        lbRecordList.Items.Add("[Không có video nào trong thư mục]");
                    }
                }
                else
                {
                    lbRecordList.Items.Add("[Thư mục Downloads không tồn tại]");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách Record: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmRecord_Load(object sender, EventArgs e)
        {
            LoadRecordList();
        }



        private void BtnChoose_Click(object sender, EventArgs e)
        {
            if (lbRecordList.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn một video để xem!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fileName = lbRecordList.SelectedItem.ToString();

            // Bỏ qua nếu người dùng bấm nhầm vào dòng thông báo lỗi
            if (fileName.StartsWith("[")) return;

            string filePath = Path.Combine(_downloadsPath, fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    // Truyền đường dẫn file vào Windows Media Player control trên Form và yêu cầu phát
                    if (axWindowsMediaPlayer1 != null)
                    {
                        axWindowsMediaPlayer1.URL = filePath;
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể phát video: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("File không còn tồn tại trên máy!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadRecordList(); // Tải lại danh sách nếu file đã bị xóa
            }
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmRecord_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (axWindowsMediaPlayer1 != null)
            {
                axWindowsMediaPlayer1.Ctlcontrols.stop();
            }
        }
    }
}
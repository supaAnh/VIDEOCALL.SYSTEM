using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SERVER.Database;

namespace SERVER.View
{
    public partial class frmRecordManage : Form
    {
        private DatabaseConnect db = new DatabaseConnect();
        private string tempVideoPath = ""; // Lưu đường dẫn file MP4 tạm thời

        public frmRecordManage()
        {
            InitializeComponent();

            // Gán các Event
            this.Load += FrmRecordManage_Load;
            this.FormClosing += FrmRecordManage_FormClosing;
            comboBoxRecordUser.SelectedIndexChanged += ComboBoxRecordUser_SelectedIndexChanged;

            // Nếu bạn kéo thả bằng giao diện thì có thể bỏ dòng gán Event này và nháy đúp vào nút
            btnChoose.Click += BtnChoose_Click;
            btnClose.Click += BtnClose_Click;
        }

        private void FrmRecordManage_Load(object sender, EventArgs e)
        {
            // Tải danh sách User có video vào ComboBox
            List<string> users = db.GetUsersWithRecords();
            comboBoxRecordUser.Items.Clear();
            foreach (string u in users)
            {
                comboBoxRecordUser.Items.Add(u);
            }
        }

        private void ComboBoxRecordUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxRecordUser.SelectedItem == null) return;
            string selectedUser = comboBoxRecordUser.SelectedItem.ToString();

            // Cập nhật ListBox tên các bản Record của User đó
            List<string> records = db.GetRecordsByUser(selectedUser);
            lbRecordList.Items.Clear();
            foreach (string record in records)
            {
                lbRecordList.Items.Add(record);
            }
        }

        private void BtnChoose_Click(object sender, EventArgs e)
        {
            if (lbRecordList.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một bản record trong danh sách để xem!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fileName = lbRecordList.SelectedItem.ToString();
            byte[] videoData = db.GetVideoData(fileName);

            if (videoData != null && videoData.Length > 0)
            {
                try
                {
                    // 1. Dừng video cũ (nếu đang phát) và nhả file
                    axWindowsMediaPlayer1.Ctlcontrols.stop();
                    axWindowsMediaPlayer1.URL = "";

                    // 2. Xóa file tạm cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(tempVideoPath) && System.IO.File.Exists(tempVideoPath))
                    {
                        try { System.IO.File.Delete(tempVideoPath); } catch { }
                    }

                    // 3. Khởi tạo đường dẫn file tạm trong thư mục Temp của Windows
                    string tempFolder = Path.Combine(Path.GetTempPath(), "VideoCallRecords");
                    if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

                    tempVideoPath = Path.Combine(tempFolder, fileName);

                    // 4. Ghi mảng byte[] từ DB thành file .mp4 thực tế
                    System.IO.File.WriteAllBytes(tempVideoPath, videoData);

                    // 5. Gắn URL vào MediaPlayer và phát
                    axWindowsMediaPlayer1.URL = tempVideoPath;
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi phát video: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Không thể tải dữ liệu video từ Database! (Có thể file rỗng)", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmRecordManage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Tắt Media Player và xóa file rác (file tạm) trước khi tắt Form để tránh nặng ổ cứng
            if (axWindowsMediaPlayer1 != null)
            {
                axWindowsMediaPlayer1.Ctlcontrols.stop();
                axWindowsMediaPlayer1.URL = "";
            }

            if (!string.IsNullOrEmpty(tempVideoPath) && System.IO.File.Exists(tempVideoPath))
            {
                try { System.IO.File.Delete(tempVideoPath); } catch { }
            }
        }


    }
}
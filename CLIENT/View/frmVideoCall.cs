using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CLIENT.View
{
    public partial class frmVideoCall : Form
    {
        private string _targetIP;
        private string _myMoniker;

        private CLIENT.Process.VideoCallProcess _videoCallLogic;

        private Dictionary<string, PictureBox> _participantVideos = new Dictionary<string, PictureBox>();

        private WaveOutEvent _waveOut;
        private BufferedWaveProvider _waveProvider;

        public frmVideoCall(string targetIP, string moniker, CLIENT.Process.VideoCallProcess videoLogic)
        {
            InitializeComponent();

            _videoCallLogic = videoLogic;
            _targetIP = targetIP;
            _myMoniker = moniker;

            // ĐĂNG KÝ SỰ KIỆN: Khi có frame mới, vẽ lên giao diện
            _videoCallLogic.OnFrameReceived += (senderIP, bmp) =>
            {
                // Kiểm tra xem người gửi frame có phải đối phương hoặc chính mình không
                if (senderIP == _targetIP || senderIP == "Me")
                {
                    UpdateFrame(senderIP, bmp);
                }
                else
                {
                    // Nếu là người mới trong group, thêm khung hình rồi mới vẽ
                    AddParticipant(senderIP);
                    UpdateFrame(senderIP, bmp);
                }
            };

            // Bắt đầu stream
            _videoCallLogic.StartStreaming(_targetIP, _myMoniker);

            // Thêm khung hình cho chính mình
            AddParticipant("Me");
        }

        //
        // Cập nhật bố cục video khi có người tham gia mới
        //
        public void AddParticipant(string participantID)
        {
            if (_participantVideos.ContainsKey(participantID)) return;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AddParticipant(participantID)));
                return;
            }

            // Tạo PictureBox mới cho người tham gia
            PictureBox pb = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };

            _participantVideos.Add(participantID, pb);
            UpdateVideoLayout();
        }

        //
        // Cập nhật bố cục video khi có người rời khỏi
        //
        private void UpdateVideoLayout()
        {
            int count = _participantVideos.Count;
            if (count == 0) return;

            // Tính số cột và hàng cần thiết (Vd: 2 người -> 2 cột 1 hàng; 4 người -> 2 cột 2 hàng)
            int cols = (int)Math.Ceiling(Math.Sqrt(count));
            int rows = (int)Math.Ceiling((double)count / cols);

            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();

            tableLayoutPanel1.ColumnCount = cols;
            tableLayoutPanel1.RowCount = rows;

            // Chia đều tỷ lệ phần trăm cho các cột
            for (int i = 0; i < cols; i++)
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));

            // Chia đều tỷ lệ phần trăm cho các hàng
            for (int i = 0; i < rows; i++)
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));

            // Thêm lại các PictureBox vào lưới
            foreach (var video in _participantVideos.Values)
            {
                tableLayoutPanel1.Controls.Add(video);
            }
        }

        //
        // Cập nhật khung hình video cho người tham gia
        //
        public void UpdateFrame(string participantID, Bitmap frame)
        {
            if (_participantVideos.ContainsKey(participantID))
            {
                var oldImg = _participantVideos[participantID].Image;
                _participantVideos[participantID].Image = frame;
                oldImg?.Dispose(); // Giải phóng bộ nhớ ảnh cũ
            }
        }

        private void btnSelectionVideoCall_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(btnSelectionVideoCall, new Point(0, btnSelectionVideoCall.Height)); // Hiển thị menu ngay dưới nút
        }

        private void thoátToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitCall();
        }

        private void ExitCall()
        {
            _videoCallLogic.SendSignal(_targetIP, "Leave"); // Gửi tín hiệu rời cuộc gọi
            _videoCallLogic.StopAll(); // Dừng tất cả các tiến trình liên quan đến video call
            this.Close(); // Đóng form
        }


        //
        // AUDIO
        //
        private void InitAudioPlayback()
        {
            // Cấu hình định dạng âm thanh khớp với phía gửi (16kHz, 16bit, Mono)
            _waveProvider = new BufferedWaveProvider(new WaveFormat(16000, 16, 1));
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_waveProvider);
            _waveOut.Play();
        }

    }
}

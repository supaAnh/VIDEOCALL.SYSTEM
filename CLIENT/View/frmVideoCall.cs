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

        public string myMoniker { get; private set; }

        private CLIENT.Process.VideoCallProcess _videoCallLogic;

        private Dictionary<string, PictureBox> _participantVideos = new Dictionary<string, PictureBox>();

        private WaveOutEvent _waveOut;
        private BufferedWaveProvider _waveProvider;

        public frmVideoCall(string targetIP, string moniker, CLIENT.Process.VideoCallProcess videoLogic)
        {
            InitializeComponent();
            _videoCallLogic = videoLogic;
            _targetIP = targetIP;
            this.myMoniker = moniker;

            // 1. Phải khởi tạo Audio Playback để tránh lỗi luồng
            InitAudioPlayback();

            _videoCallLogic.OnFrameReceived += (senderIP, bmp) =>
            {
                if (this.IsDisposed) return;
                this.BeginInvoke(new Action(() => {
                    // Nếu là người mới (không phải "Me" và chưa có trong danh sách)
                    if (!_participantVideos.ContainsKey(senderIP))
                    {
                        AddParticipant(senderIP);
                    }
                    UpdateFrame(senderIP, bmp);
                }));
            };

            // 2. Bắt đầu stream
            _videoCallLogic.StartStreaming(_targetIP, myMoniker);

            // 3. Thêm chính mình vào giao diện
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

            PictureBox pb = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill
            };

            _participantVideos.Add(participantID, pb);

            // Gọi cập nhật bố cục ngay lập tức
            UpdateVideoLayout();
        }

        //
        // Cập nhật bố cục video
        //
        private void UpdateVideoLayout()
        {
            int count = _participantVideos.Count;
            if (count == 0) return;

            // Tính toán số cột và hàng (VD: 2 người -> 2 cột, 1 hàng)
            int cols = (count <= 1) ? 1 : (count <= 2) ? 2 : (int)Math.Ceiling(Math.Sqrt(count));
            int rows = (int)Math.Ceiling((double)count / cols);

            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();

            tableLayoutPanel1.ColumnCount = cols;
            tableLayoutPanel1.RowCount = rows;

            // Chia đều 100% không gian cho các cột và hàng
            for (int i = 0; i < cols; i++)
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
            for (int i = 0; i < rows; i++)
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));

            int current = 0;
            foreach (var pb in _participantVideos.Values)
            {
                int r = current / cols;
                int c = current % cols;
                tableLayoutPanel1.Controls.Add(pb, c, r);
                current++;
            }

            tableLayoutPanel1.ResumeLayout();
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

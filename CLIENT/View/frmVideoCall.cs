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

            _videoCallLogic.OnFrameReceived += (senderIP, bmp) =>
            {
                this.BeginInvoke(new Action(() => {
                    // LUÔN KIỂM TRA: Nếu chưa có PictureBox cho IP này thì phải tạo mới
                    if (!_participantVideos.ContainsKey(senderIP))
                    {
                        AddParticipant(senderIP);
                    }

                    // Cập nhật frame sau khi đã chắc chắn có PictureBox
                    UpdateFrame(senderIP, bmp);
                }));
            };

            _videoCallLogic.StartStreaming(_targetIP, _myMoniker);
            AddParticipant("Me"); // Thêm khung hình của chính mình
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
                Dock = DockStyle.Fill // Thiết lập ngay khi tạo
            };

            _participantVideos.Add(participantID, pb);

            // Gọi hàm cập nhật layout ngay sau khi thêm
            UpdateVideoLayout();
        }

        //
        // Cập nhật bố cục video
        //
        private void UpdateVideoLayout()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateVideoLayout));
                return;
            }

            int count = _participantVideos.Count;
            if (count == 0) return;

            int cols = (count <= 1) ? 1 : (count <= 2) ? 2 : (int)Math.Ceiling(Math.Sqrt(count));
            int rows = (int)Math.Ceiling((double)count / cols);

            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();

            tableLayoutPanel1.ColumnCount = cols;
            tableLayoutPanel1.RowCount = rows;

            // Thiết lập tỷ lệ phần trăm cho cột và hàng
            for (int i = 0; i < cols; i++)
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
            for (int i = 0; i < rows; i++)
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));

            // Quan trọng: Gán PictureBox vào đúng vị trí lưới
            int current = 0;
            foreach (var pb in _participantVideos.Values)
            {
                pb.Dock = DockStyle.Fill;
                pb.Margin = new Padding(2);
                int r = current / cols;
                int c = current % cols;
                tableLayoutPanel1.Controls.Add(pb, c, r); // Thêm kèm vị trí cột, hàng
                current++;
            }

            tableLayoutPanel1.ResumeLayout();
            tableLayoutPanel1.Refresh(); // Buộc vẽ lại giao diện
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

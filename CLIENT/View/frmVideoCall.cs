using COMMON.DTO;
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
        // IP của đối tác đang gọi đến hoặc gọi đi, được truyền vào từ bên ngoài để dễ dàng quản lý và gửi tín hiệu
        private string _targetIP;

        // Moniker của chính mình, thường là tên người dùng hoặc IP, được truyền vào từ bên ngoài để dễ dàng quản lý và hiển thị
        public string myMoniker { get; private set; }

        // Logic xử lý cuộc gọi video, được truyền vào từ bên ngoài để tách biệt rõ ràng giữa UI và Network
        private CLIENT.Process.VideoCallProcess _videoCallLogic;

        // Quản lý các PictureBox hiển thị video của từng người tham gia, key là participantID (thường là IP hoặc Moniker)
        private Dictionary<string, PictureBox> _participantVideos = new Dictionary<string, PictureBox>();


        // Audio Playback
        private WaveOutEvent _waveOut;
        private BufferedWaveProvider _waveProvider;

        // Trạng thái Camera và Microphone
        private bool _isCamOn;
        private bool _isMicOn;

        // Ghi hình cuộc gọi
        private System.Windows.Forms.Timer _recordTimer;
        private bool _isRecordingForm = false;
        private string _currentRecordPath = "";
        private ClientSocketConnect _clientSocket; // gửi tín hiệu kết thúc ghi âm

        // Dùng để theo dõi những người đã rời đi, tránh việc thêm lại vào giao diện nếu họ vô tình gửi khung hình sau khi đã rời
        private HashSet<string> _leftParticipants = new HashSet<string>();


        public frmVideoCall(string targetIP, string moniker, CLIENT.Process.VideoCallProcess videoLogic, bool isCamOn, bool isMicOn)
        {
            InitializeComponent();
            _videoCallLogic = videoLogic;
            _targetIP = targetIP;
            this.myMoniker = moniker;

            // Lưu trạng thái ban đầu
            _isCamOn = isCamOn;
            _isMicOn = isMicOn;

            // 1. Phải khởi tạo Audio Playback để tránh lỗi luồng
            InitAudioPlayback();

            // Dùng Delegate chính xác để tránh leak memory và cập nhật khung hình
            _videoCallLogic.OnAudioReceived += OnAudioReceivedHandler;
            _videoCallLogic.OnFrameReceived += OnFrameReceivedHandler;
            _videoCallLogic.OnParticipantLeft += RemoveParticipant;

            // 2. Bắt đầu stream
            _videoCallLogic.StartStreaming(_targetIP, myMoniker, _isCamOn, _isMicOn);

            // 3. Thêm chính mình vào giao diện
            AddParticipant("Me");


            _recordTimer = new System.Windows.Forms.Timer();
            _recordTimer.Interval = 100;
            _recordTimer.Tick += RecordTimer_Tick;
        }

        //
        // Cập nhật bố cục video khi có người tham gia mới
        //
        public void AddParticipant(string participantID)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AddParticipant(participantID)));
                return;
            }

            // Xóa khỏi danh sách chặn nếu người này thực sự gọi/join lại
            if (_leftParticipants.Contains(participantID))
            {
                _leftParticipants.Remove(participantID);
            }

            if (_participantVideos.ContainsKey(participantID)) return;

            PictureBox pb = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill
            };

            // TẠO COMPONENT LABEL BẰNG CODE ĐỂ ĐỊNH DANH
            Label lblUserName = new Label
            {
                Text = participantID,
                AutoSize = true,
                BackColor = Color.FromArgb(128, 0, 0, 0), // Nền đen mờ
                ForeColor = Color.White,                  // Chữ trắng
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(5, 5),
                Padding = new Padding(2)
            };
            pb.Controls.Add(lblUserName);

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
                PictureBox pb = _participantVideos[participantID];
                var oldImg = pb.Image;

                pb.Image = frame; // Gán ảnh mới

                // CỰC KỲ QUAN TRỌNG: Giải phóng ảnh cũ để tránh đầy RAM
                if (oldImg != null)
                {
                    oldImg.Dispose();
                }
            }
            else
            {
                // Nếu người dùng không tồn tại (đã thoát), dispose frame mới nhận để tránh rác
                frame?.Dispose();
            }
        }

        // Xử lý khi nhận được Audio (Thêm mới)
        public void OnAudioReceivedHandler(byte[] audioData)
        {
            if (_waveProvider != null)
            {
                try
                {
                    _waveProvider.AddSamples(audioData, 0, audioData.Length);
                }
                catch { /* Bỏ qua lỗi buffer đầy nếu cần */ }
            }
        }

        // Khởi tạo hệ thống phát âm thanh
        private void InitAudioPlayback()
        {
            try
            {
                // Buffer 16kHz, 16bit, Mono
                _waveProvider = new BufferedWaveProvider(new WaveFormat(16000, 16, 1));
                _waveProvider.DiscardOnBufferOverflow = true; // Tránh crash khi mạng lag dồn gói tin

                _waveOut = new WaveOutEvent();
                _waveOut.Init(_waveProvider);
                _waveOut.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo Audio: " + ex.Message);
            }
        }

        // Xử lý khi nhận được khung hình video mới
        private void OnFrameReceivedHandler(string senderIP, Bitmap bmp)
        {
            if (this.IsDisposed) return;

            // Invoke an toàn
            this.BeginInvoke(new Action(() =>
            {
                // Nếu user đã rời phòng thì huỷ bỏ ngay các khung hình đến trễ
                if (_leftParticipants.Contains(senderIP))
                {
                    bmp?.Dispose();
                    return;
                }

                if (!_participantVideos.ContainsKey(senderIP))
                {
                    AddParticipant(senderIP);
                }
                UpdateFrame(senderIP, bmp);
            }));
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            ExitCall();
        }


        //
        // AUDIO
        //


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Gửi tín hiệu "Leave" cho server trước khi đóng
            // Sử dụng Task.Run để không bị treo giao diện khi gửi mạng
            Task.Run(() => _videoCallLogic.SendSignal(_targetIP, "Leave"));

            // Dọn dẹp tài nguyên
            CleanUpResources();

            base.OnFormClosing(e);
        }

        private void RemoveParticipant(string participantID)
        {
            // Đảm bảo chạy trên luồng UI
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => RemoveParticipant(participantID)));
                return;
            }

            // Đưa người này vào danh sách chặn để block các Frame delay
            _leftParticipants.Add(participantID);

            // Kiểm tra xem ID này có đang hiển thị không
            if (_participantVideos.ContainsKey(participantID))
            {
                try
                {
                    // Lấy PictureBox ra
                    PictureBox pb = _participantVideos[participantID];

                    // Xóa khỏi giao diện (TableLayoutPanel)
                    tableLayoutPanel1.Controls.Remove(pb);

                    // Giải phóng tài nguyên
                    pb.Dispose();

                    // Xóa khỏi danh sách quản lý
                    _participantVideos.Remove(participantID);

                    // Cập nhật lại bố cục lưới video
                    UpdateVideoLayout();

                    // --- LOGIC KẾT THÚC TỰ ĐỘNG ---
                    // Nếu danh sách chỉ còn 1 người (là chính mình "Me"), thì kết thúc
                    if (_participantVideos.Count <= 1)
                    {
                        MessageBox.Show("Người chat đã rời đi. Kết thúc cuộc gọi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close(); // Đóng form, việc này sẽ kích hoạt OnFormClosing
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi RemoveParticipant: " + ex.Message);
                }
            }
        }

        private void ExitCall()
        {
            this.Close();
        }

        private void CleanUpResources()
        {
            try
            {
                // Hủy đăng ký sự kiện để tránh gọi lại vào Form đã đóng (Lỗi ObjectDisposed)
                _videoCallLogic.OnFrameReceived -= OnFrameReceivedHandler;
                _videoCallLogic.OnParticipantLeft -= RemoveParticipant; // <--- Thêm dòng này

                _videoCallLogic.StopAll();

                if (_waveOut != null)
                {
                    _waveOut.Stop();
                    _waveOut.Dispose();
                    _waveOut = null;
                }

                // Giải phóng ảnh trong các PictureBox
                foreach (var pb in _participantVideos.Values)
                {
                    if (pb.Image != null) pb.Image.Dispose();
                    pb.Dispose();
                }
                _participantVideos.Clear();
            }
            catch { }
        }

        private void UpdateButtonState()
        {
            // Cập nhật màu sắc nút để người dùng biết đang Bật hay Tắt
            btnCamera.BackColor = _isCamOn ? Color.LightGreen : Color.IndianRed;
            btnCamera.Text = _isCamOn ? "Tắt Cam" : "Bật Cam";

            btnMicrophone.BackColor = _isMicOn ? Color.LightGreen : Color.IndianRed;
            btnMicrophone.Text = _isMicOn ? "Tắt Mic" : "Bật Mic";
        }

        private void btnMicrophone_Click(object sender, EventArgs e)
        {
            _isMicOn = !_isMicOn; // Đảo trạng thái
            _videoCallLogic.ToggleMic(_targetIP, _isMicOn);
            UpdateButtonState();
        }

        private void btnCamera_Click(object sender, EventArgs e)
        {
            _isCamOn = !_isCamOn; // Đảo trạng thái
            _videoCallLogic.ToggleCamera(_isCamOn);
            UpdateButtonState();
        }




        //
        //  --- QUAY MÀN HÌNH CUỘC GỌI ---
        //

        private void RecordTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // làm tròn trừ đi 1 pixel nếu kích thước hiện tại của Form đang là số lẻ
                int safeWidth = this.Width % 2 == 0 ? this.Width : this.Width - 1;
                int safeHeight = this.Height % 2 == 0 ? this.Height : this.Height - 1;

                // Chụp lại giao diện với kích thước an toàn
                Bitmap bmp = new Bitmap(safeWidth, safeHeight);
                this.DrawToBitmap(bmp, new Rectangle(0, 0, safeWidth, safeHeight));

                _videoCallLogic.AddFrameToRecord(bmp);
                bmp.Dispose();
            }
            catch { }
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (!_isRecordingForm)
            {
                // 1. BẮT ĐẦU GHI
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadsPath)) Directory.CreateDirectory(downloadsPath);

                // SỬA LỖI TẠI ĐÂY: Lọc bỏ các ký tự không hợp lệ đối với tên file trong myMoniker
                string safeMoniker = string.Join("_", myMoniker.Split(Path.GetInvalidFileNameChars()));

                string fileName = $"Record_{safeMoniker}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
                _currentRecordPath = Path.Combine(downloadsPath, fileName);

                _videoCallLogic.StartFormRecording(_currentRecordPath);

                // KIỂM TRA: Nếu VideoCallLogic đã bật IsRecording thành công thì mới chạy UI
                if (_videoCallLogic._isRecording)
                {
                    _recordTimer.Start();
                    _isRecordingForm = true;

                    btnRecord.Text = "Dừng ghi";
                    btnRecord.BackColor = Color.Red;

                    // Gửi thông báo cho đối tác
                    _videoCallLogic.SendSignal(_targetIP, "RecordStart");
                }
            }
            else
            {
                // 2. DỪNG GHI VÀ UPLOAD
                _recordTimer.Stop();
                _videoCallLogic.StopRecordingOnly();
                _isRecordingForm = false;

                btnRecord.Text = "Ghi hình";
                btnRecord.BackColor = Color.LightGray;

                // Tăng thời gian chờ lên 1.5 giây để FFMPEG chắc chắn đã nhả file MP4 hoàn toàn
                System.Threading.Thread.Sleep(1500);

                try
                {
                    string directoryPath = Path.GetDirectoryName(_currentRecordPath);

                    if (File.Exists(_currentRecordPath))
                    {
                        // Mở thư mục và bôi đen file sử dụng ProcessStartInfo để tránh lỗi ShellExecute
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"/select,\"{_currentRecordPath}\"",
                            UseShellExecute = true
                        });
                    }
                    else if (Directory.Exists(directoryPath))
                    {
                        // Nếu lưu chậm chưa thấy file, mở thẳng thư mục
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"\"{directoryPath}\"",
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Thông báo lỗi cụ thể nếu hệ điều hành từ chối mở
                    MessageBox.Show("Không thể tự động mở thư mục: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                MessageBox.Show($"Đã dừng ghi hình!\nVideo được lưu tại: {_currentRecordPath}", "Ghi hình thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Gửi file cho Server lưu DB
                UploadRecordToServer(_currentRecordPath, Path.GetFileName(_currentRecordPath));
            }
        }

        private void UploadRecordToServer(string filePath, string fileName)
        {
            // Chạy ngầm để việc đọc và gửi file MP4 dung lượng lớn không làm đơ Form
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    if (!System.IO.File.Exists(filePath)) return;
                    byte[] fileData = System.IO.File.ReadAllBytes(filePath);

                    // Đóng gói tên file và dữ liệu video vào FilePackageDTO
                    var fileDto = new FilePackageDTO
                    {
                        FileName = fileName,
                        FileData = fileData
                    };

                    byte[] rawData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(fileDto);

                    byte[] targetIPBytes = Encoding.UTF8.GetBytes(_targetIP);
                    byte[] ipLengthBytes = BitConverter.GetBytes(targetIPBytes.Length);

                    byte[] finalPayload = new byte[4 + targetIPBytes.Length + rawData.Length];
                    Buffer.BlockCopy(ipLengthBytes, 0, finalPayload, 0, 4);
                    Buffer.BlockCopy(targetIPBytes, 0, finalPayload, 4, targetIPBytes.Length);
                    Buffer.BlockCopy(rawData, 0, finalPayload, 4 + targetIPBytes.Length, rawData.Length);

                    // GỌI HÀM GỬI LÊN SERVER
                    _videoCallLogic.SendRecordToServer(finalPayload);
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() => MessageBox.Show("Lỗi upload video: " + ex.Message)));
                }
            });
        }

        
    }
}

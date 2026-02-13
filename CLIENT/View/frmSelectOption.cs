using AForge.Video.DirectShow;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CLIENT.View
{
    public partial class frmSelectOption : Form
    {

        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _previewSource;

        private List<MMDevice> _inputDevices;
        private List<MMDevice> _outputDevices;

        private AudioFileReader _audioFile;
        private WaveOutEvent _outputDevice;

        // NAudio để thử Mic
        private WaveInEvent _waveIn;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedMoniker { get; set; } // ID Camera truyền ra frmMain

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedInputId { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedOutputId { get; set; }

        public frmSelectOption()
        {
            InitializeComponent();
        }

        private void frmSelectOption_Load(object sender, EventArgs e)
        {
            // 1. Nạp danh sách Camera
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in _videoDevices)
            {
                cbCamera.Items.Add(device.Name);
            }
            if (cbCamera.Items.Count > 0) cbCamera.SelectedIndex = 0;

            // 2. Nạp thiết bị âm thanh
            var enumerator = new MMDeviceEnumerator();

            // Microphone (Input)
            var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            _inputDevices = captureDevices.ToList();
            foreach (var dev in _inputDevices) cbInput.Items.Add(dev.FriendlyName);
            if (cbInput.Items.Count > 0) cbInput.SelectedIndex = 0;

            // Loa (Output)
            var renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            _outputDevices = renderDevices.ToList();
            foreach (var dev in _outputDevices) cbOutput.Items.Add(dev.FriendlyName);
            if (cbOutput.Items.Count > 0) cbOutput.SelectedIndex = 0;

            // Đăng ký sự kiện nút bấm
            btnCamera.Click += (s, ev) => TogglePreview();
            btnMicrophone.Click += (s, ev) => ToggleMicTest();
        }


        private void btnExit_Click(object sender, EventArgs e)
        {

            StopPreview();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            if (cbCamera.SelectedIndex >= 0)
                this.SelectedMoniker = _videoDevices[cbCamera.SelectedIndex].MonikerString;

            if (cbInput.SelectedIndex >= 0)
                this.SelectedInputId = _inputDevices[cbInput.SelectedIndex].ID;

            if (cbOutput.SelectedIndex >= 0)
                this.SelectedOutputId = _outputDevices[cbOutput.SelectedIndex].ID;

            StopPreview();
            if (_waveIn != null) ToggleMicTest();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }



        // --- XỬ LÝ CAMERA PREVIEW ---
        private void TogglePreview()
        {
            if (_previewSource != null && _previewSource.IsRunning)
            {
                StopPreview();
                btnCamera.Text = "Mở cam";
            }
            else
            {
                if (cbCamera.SelectedIndex < 0) return;

                _previewSource = new VideoCaptureDevice(_videoDevices[cbCamera.SelectedIndex].MonikerString);
                _previewSource.NewFrame += (s, e) =>
                {
                    // Hiển thị khung hình lên PictureBox
                    Bitmap bmp = (Bitmap)e.Frame.Clone();
                    pictureBoxCamPreview.Image = bmp;
                };
                _previewSource.Start();
                btnCamera.Text = "Đóng cam";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Dừng camera preview nếu đang chạy
            StopPreview();

            // Dừng mic test nếu đang chạy
            if (_waveIn != null)
            {
                try
                {
                    _waveIn.StopRecording();
                    _waveIn.Dispose();
                }
                catch { }
                _waveIn = null;
            }

            base.OnFormClosing(e);
        }

        private void StopPreview()
        {
            if (_previewSource != null)
            {
                if (_previewSource.IsRunning)
                {
                    _previewSource.SignalToStop();
                    _previewSource.WaitForStop(); // Bắt buộc đợi tắt hẳn
                }
                _previewSource = null;
            }
            pictureBoxCamPreview.Image = null;
        }



        // --- XỬ LÝ THỬ MICROPHONE ---
        private void ToggleMicTest()
        {
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
                progessBarMic.Value = 0; // Reset thanh khi đóng
                btnMicrophone.Text = "Mở mic";
            }
            else
            {
                try
                {
                    _waveIn = new WaveInEvent();
                    _waveIn.DeviceNumber = cbInput.SelectedIndex; // Lấy thiết bị từ ComboBox
                    _waveIn.WaveFormat = new WaveFormat(44100, 1); // 44.1kHz, Mono

                    _waveIn.DataAvailable += (s, e) =>
                    {
                        float max = 0;
                        // Duyệt qua dữ liệu byte để tìm giá trị đỉnh (Peak)
                        for (int i = 0; i < e.BytesRecorded; i += 2)
                        {
                            short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                            float sample32 = Math.Abs(sample / 32768f);
                            if (sample32 > max) max = sample32;
                        }

                        // Cập nhật giao diện từ luồng phụ
                        this.BeginInvoke(new Action(() =>
                        {
                            // Nhân với 100 để chuyển sang thang điểm phần trăm của ProgressBar
                            int volumeValue = (int)(max * 100);
                            progessBarMic.Value = Math.Min(100, volumeValue);
                        }));
                    };

                    _waveIn.StartRecording();
                    btnMicrophone.Text = "Đóng mic";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thử Mic: " + ex.Message);
                }
            }
        }

        private void btnOutputTest_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy thư mục gốc nơi ứng dụng đang chạy (bin/Debug/net10.0-windows/)
                string appDir = AppDomain.CurrentDomain.BaseDirectory;

                // Kết hợp đường dẫn: Thư mục thực thi + Sound + soundtest.mp3
                string testFilePath = Path.Combine(appDir, "Sound", "soundtest.mp3");

                // Kiểm tra sự tồn tại của file trước khi phát
                if (!File.Exists(testFilePath))
                {
                    MessageBox.Show("Vẫn không tìm thấy file âm thanh!\nĐường dẫn đang tìm: " + testFilePath,
                                    "Lỗi đường dẫn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Khởi tạo NAudio để phát
                _audioFile = new AudioFileReader(testFilePath);
                _outputDevice = new WaveOutEvent { DeviceNumber = cbOutput.SelectedIndex };
                _outputDevice.Init(_audioFile);
                _outputDevice.Play();

                btnOutputTest.Text = "Dừng thử";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        
    }
}

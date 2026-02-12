using Accord.Video;
using AForge.Video.DirectShow;
using COMMON.DTO;
using COMMON.Security;
using NAudio.Wave;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace CLIENT.Process
{
    public class VideoCallProcess
    {
        private ClientSocketConnect _client;
        private VideoCaptureDevice _videoSource;
        private WaveInEvent _waveIn;

        // Sử dụng đường dẫn đầy đủ để tránh xung đột với namespace CLIENT.Process
        private System.Diagnostics.Process _ffmpegProcess;
        private Stream _ffmpegStream;
        private bool _isRecording = false;

        public event Action<string, Bitmap> OnFrameReceived;
        public event Action<byte[]> OnAudioReceived;

        public VideoCallProcess(ClientSocketConnect client)
        {
            _client = client;
        }

        /// <summary>
        /// Gửi tín hiệu điều khiển cuộc gọi (Invite, Ringing, Busy, End...)
        /// </summary>
        // CLIENT/Process/VideoCallProcess.cs

        public void SendSignal(string target, string status)
        {
            try
            {
                // Thêm "| " ở cuối để tránh lỗi IndexOutOfRangeException tại Server
                string signalData = $"{target}|{status}| ";
                byte[] content = Encoding.UTF8.GetBytes(signalData);
                _client.Send(new DataPackage(PackageType.VideoCall, content).Pack());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi gửi Signal: " + ex.Message);
            }
        }

        /// <summary>
        /// Phương thức tương thích với các gọi cũ trong frmMain
        /// </summary>
        public void SendVideoCallSignal(string target, string status) => SendSignal(target, status);

        /// <summary>
        /// Bắt đầu lấy hình ảnh từ Camera và âm thanh từ Mic để stream
        /// </summary>
        public void StartStreaming(string target, string monikerString)
        {
            if (string.IsNullOrEmpty(monikerString)) return;

            try
            {
                _videoSource = new VideoCaptureDevice(monikerString);
                _videoSource.NewFrame += (s, e) =>
                {
                    // Tạo bản sao frame để xử lý, tránh lỗi truy cập đồng thời
                    using (Bitmap bmp = (Bitmap)e.Frame.Clone())
                    {
                        // Nếu đang ghi hình, đẩy frame vào stdin của ffmpeg.exe
                        if (_isRecording && _ffmpegStream != null)
                        {
                            try
                            {
                                // Lưu frame dưới dạng BMP vào luồng pipe của ffmpeg
                                bmp.Save(_ffmpegStream, ImageFormat.Bmp);
                            }
                            catch { /* Xử lý lỗi pipe nếu ffmpeg bị đóng đột ngột */ }
                        }

                        // Gửi frame qua mạng cho đối phương
                        SendFrame(target, bmp);
                    }
                };
                _videoSource.Start();

                // Bắt đầu thu âm thanh song song
                StartAudioCapture(target);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi StartStreaming: " + ex.Message);
            }
        }

        private void SendFrame(string target, Bitmap bmp)
        {
            try
            {
                // Hiển thị frame lên màn hình của chính mình trước
                OnFrameReceived?.Invoke("Me", (Bitmap)bmp.Clone());

                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    byte[] rawData = ms.ToArray();
                    byte[] encryptedData = AES_Service.Encrypt(rawData, _client.AesKey);

                    string payload = $"{target}|Frame|{Convert.ToBase64String(encryptedData)}";
                    _client.Send(new DataPackage(PackageType.VideoCall, Encoding.UTF8.GetBytes(payload)).Pack());
                }
            }
            catch { }
        }

        public void StartAudioCapture(string targetIP)
        {
            try
            {
                _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 16, 1) };
                _waveIn.DataAvailable += (s, e) =>
                {
                    byte[] encryptedAudio = AES_Service.Encrypt(e.Buffer, _client.AesKey);
                    string payload = $"{targetIP}|Audio|{Convert.ToBase64String(encryptedAudio)}";
                    _client.Send(new DataPackage(PackageType.VideoCall, Encoding.UTF8.GetBytes(payload)).Pack());
                };
                _waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi thu âm: " + ex.Message);
            }
        }

        /// <summary>
        /// Ghi hình cuộc gọi bằng cách gọi ffmpeg.exe bên ngoài
        /// </summary>
        public void StartRecording(string outputPath, int width, int height)
        {
            if (_isRecording) return;

            try
            {
                _ffmpegProcess = new System.Diagnostics.Process();
                _ffmpegProcess.StartInfo.FileName = "ffmpeg.exe";

                // Cấu hình Arguments: Nhận chuỗi ảnh BMP từ pipe và nén thành MP4
                _ffmpegProcess.StartInfo.Arguments = $"-f image2pipe -vcodec bmp -i - -c:v libx264 -preset ultrafast -pix_fmt yuv420p -y \"{outputPath}\"";

                _ffmpegProcess.StartInfo.UseShellExecute = false;
                _ffmpegProcess.StartInfo.RedirectStandardInput = true; //  Để đẩy dữ liệu bitmap vào
                _ffmpegProcess.StartInfo.CreateNoWindow = true;

                _ffmpegProcess.Start();
                _ffmpegStream = _ffmpegProcess.StandardInput.BaseStream;
                _isRecording = true;

                System.Diagnostics.Debug.WriteLine("Đã bắt đầu ghi hình vào: " + outputPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khởi động ffmpeg: " + ex.Message);
            }
        }

        /// <summary>
        /// Dừng tất cả các luồng xử lý (Camera, Mic, Ghi hình)
        /// </summary>
        public void StopAll()
        {
            // 1. Dừng Camera
            if (_videoSource != null)
            {
                if (_videoSource.IsRunning) _videoSource.SignalToStop();
                _videoSource = null;
            }

            // 2. Dừng Mic
            if (_waveIn != null)
            {
                try { _waveIn.StopRecording(); } catch { }
                _waveIn.Dispose();
                _waveIn = null;
            }

            // 3. Dừng Ghi hình
            if (_isRecording)
            {
                _isRecording = false;
                if (_ffmpegStream != null)
                {
                    try
                    {
                        _ffmpegStream.Flush();
                        _ffmpegStream.Close();
                    }
                    catch { }
                    _ffmpegStream = null;
                }

                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.WaitForExit(1000);
                    try { _ffmpegProcess.Kill(); } catch { }
                    _ffmpegProcess.Dispose();
                    _ffmpegProcess = null;
                }
            }
        }






        //
        // Phương thức để xử lý dữ liệu thô nhận được từ Server (gọi từ frmMain)
        //
        public void ProcessIncomingVideoData(string senderIP, string action, string rawPayload)
        {
            try
            {
                // 1. Giải mã dữ liệu nhận được
                byte[] encryptedData = Convert.FromBase64String(rawPayload);
                byte[] decryptedData = AES_Service.Decrypt(encryptedData, _client.AesKey);

                if (action == "Frame")
                {
                    using (MemoryStream ms = new MemoryStream(decryptedData))
                    {
                        Bitmap bmp = new Bitmap(ms);
                        // 2. Kích hoạt sự kiện để báo cho Form vẽ hình
                        OnFrameReceived?.Invoke(senderIP, bmp);
                    }
                }
                else if (action == "Audio")
                {
                    OnAudioReceived?.Invoke(decryptedData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi xử lý video đến: " + ex.Message);
            }
        }
    }
}
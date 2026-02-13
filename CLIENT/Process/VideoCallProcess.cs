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

        // Sự kiện để báo cho UI biết có hình ảnh hoặc âm thanh mới
        public event Action<string, Bitmap> OnFrameReceived;
        public event Action<byte[]> OnAudioReceived;

        // Sự kiện khi có người rời cuộc gọi
        public event Action<string> OnParticipantLeft;

        public VideoCallProcess(ClientSocketConnect client)
        {
            _client = client;

            // QUAN TRỌNG: Đăng ký nhận dữ liệu từ Socket ngay khi khởi tạo
            _client.OnRawDataReceived += Client_OnRawDataReceived;
        }

        // --- XỬ LÝ NHẬN DỮ LIỆU TỪ SOCKET ---
        private void Client_OnRawDataReceived(byte[] packetData)
        {
            try
            {
                // 1. Giải gói tin (Unpack)
                var package = DataPackage.Unpack(packetData);

                // 2. Chỉ xử lý nếu là gói tin VideoCall
                if (package.Type == PackageType.VideoCall)
                {
                    // 3. Tách payload: TargetIP|Action|Base64Data
                    string rawContent = Encoding.UTF8.GetString(package.Content);
                    string[] parts = rawContent.Split('|');

                    if (parts.Length >= 3)
                    {
                        string senderIP = parts[0];
                        string action = parts[1]; // "Frame" hoặc "Audio"
                        string payload = parts[2]; // Dữ liệu mã hóa Base64

                        // Gọi hàm xử lý chi tiết
                        ProcessIncomingVideoData(senderIP, action, payload);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi Client_OnRawDataReceived: " + ex.Message);
            }
        }

        /// <summary>
        /// Xử lý dữ liệu hình ảnh/âm thanh đã nhận và giải mã
        /// </summary>
        public void ProcessIncomingVideoData(string senderIP, string action, string rawPayload)
        {
            try
            {
                // 1. Xử lý các tín hiệu điều khiển (Không cần giải mã Base64/AES)
                if (action == "Leave")
                {
                    OnParticipantLeft?.Invoke(senderIP);
                    return; // Thoát luôn sau khi xử lý xong
                }
                else if (action == "Request" || action == "Accept" || action == "Refuse")
                {
                    // Có thể bắn sự kiện cho UI xử lý hiển thị thông báo/trạng thái
                    return;
                }

                // 2. Chỉ giải mã khi là dữ liệu Media (Frame/Audio) và payload có dữ liệu
                if (!string.IsNullOrWhiteSpace(rawPayload))
                {
                    try
                    {
                        byte[] encryptedData = Convert.FromBase64String(rawPayload);
                        byte[] decryptedData = AES_Service.Decrypt(encryptedData, _client.AesKey);

                        if (action == "Frame")
                        {
                            using (MemoryStream ms = new MemoryStream(decryptedData))
                            {
                                using (Bitmap tempBmp = new Bitmap(ms))
                                {
                                    Bitmap safeFrame = new Bitmap(tempBmp);
                                    OnFrameReceived?.Invoke(senderIP, safeFrame);
                                }
                            }
                        }
                        else if (action == "Audio")
                        {
                            OnAudioReceived?.Invoke(decryptedData);
                        }
                    }
                    catch (FormatException)
                    {
                        // Bỏ qua lỗi Base64 nếu payload không đúng định dạng
                    }
                    catch (System.Security.Cryptography.CryptographicException)
                    {
                        // Bỏ qua lỗi giải mã
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi xử lý video đến: " + ex.Message);
            }
        }

        // --- CÁC CHỨC NĂNG GỬI ĐI ---

        /// <summary>
        /// Gửi tín hiệu điều khiển cuộc gọi (Invite, Ringing, Busy, End...)
        /// </summary>
        public void SendSignal(string target, string status)
        {
            try
            {
                // Thêm "| " ở cuối để đảm bảo split không lỗi
                string signalData = $"{target}|{status}| ";
                byte[] content = Encoding.UTF8.GetBytes(signalData);
                _client.Send(new DataPackage(PackageType.VideoCall, content).Pack());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi gửi Signal: " + ex.Message);
            }
        }

        public void SendVideoCallSignal(string target, string status) => SendSignal(target, status);

        /// <summary>
        /// Bắt đầu lấy hình ảnh từ Camera và âm thanh từ Mic để stream
        /// </summary>
        public void StartStreaming(string target, string monikerString, bool isCamOn, bool isMicOn)
        {
            if (string.IsNullOrEmpty(monikerString)) return;

            try
            {
                // 1. Cấu hình Camera nhưng CHƯA START nếu isCamOn = false
                _videoSource = new VideoCaptureDevice(monikerString);
                _videoSource.NewFrame += (s, e) =>
                {
                    using (Bitmap bmp = (Bitmap)e.Frame.Clone())
                    {
                        if (_isRecording && _ffmpegStream != null)
                        {
                            try { bmp.Save(_ffmpegStream, ImageFormat.Bmp); } catch { }
                        }
                        SendFrame(target, bmp);
                    }
                };

                // Chỉ bật nếu được yêu cầu
                if (isCamOn)
                {
                    _videoSource.Start();
                }

                // 2. Cấu hình Mic
                if (isMicOn)
                {
                    StartAudioCapture(target);
                }
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
                // Hiển thị frame lên màn hình của chính mình trước (Self-View)
                OnFrameReceived?.Invoke("Me", (Bitmap)bmp.Clone());

                using (MemoryStream ms = new MemoryStream())
                {
                    // Nén ảnh sang JPEG để giảm dung lượng mạng
                    bmp.Save(ms, ImageFormat.Jpeg);
                    byte[] rawData = ms.ToArray();

                    // Mã hóa AES
                    byte[] encryptedData = AES_Service.Encrypt(rawData, _client.AesKey);

                    // Đóng gói: Target|Frame|Base64Data
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
                // Cấu hình mic: 16kHz, 16bit, Mono
                _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 16, 1) };
                _waveIn.DataAvailable += (s, e) =>
                {
                    if (e.BytesRecorded > 0)
                    {
                        // Chỉ lấy đúng số byte thu được
                        byte[] audioChunk = new byte[e.BytesRecorded];
                        Array.Copy(e.Buffer, 0, audioChunk, 0, e.BytesRecorded);

                        byte[] encryptedAudio = AES_Service.Encrypt(audioChunk, _client.AesKey);
                        string payload = $"{targetIP}|Audio|{Convert.ToBase64String(encryptedAudio)}";
                        _client.Send(new DataPackage(PackageType.VideoCall, Encoding.UTF8.GetBytes(payload)).Pack());
                    }
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
                _ffmpegProcess.StartInfo.RedirectStandardInput = true;
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
            // 1. Ngắt kết nối sự kiện nhận dữ liệu
            _client.OnRawDataReceived -= Client_OnRawDataReceived;

            // 2. TẮT CAMERA
            if (_videoSource != null)
            {
                if (_videoSource.IsRunning)
                {
                    // Quan trọng: Hủy sự kiện NewFrame để không xử lý ảnh mới nữa
                    _videoSource.SignalToStop();
                    _videoSource.WaitForStop();
                }
                _videoSource = null;
            }

            // 3. TẮT MIC
            if (_waveIn != null)
            {
                try
                {
                    _waveIn.StopRecording();
                    // Dispose mic đôi khi cần 1 chút thời gian để nhả driver
                    System.Threading.Thread.Sleep(100);
                    _waveIn.Dispose();
                }
                catch { }
                _waveIn = null;
            }

            // 3. DỪNG GHI HÌNH (FFMPEG)
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

                if (_ffmpegProcess != null)
                {
                    try
                    {
                        if (!_ffmpegProcess.HasExited)
                        {
                            _ffmpegProcess.Kill(); // Đảm bảo ffmpeg dừng hẳn
                            _ffmpegProcess.WaitForExit();
                        }
                    }
                    catch { }
                    _ffmpegProcess.Dispose();
                    _ffmpegProcess = null;
                }
            }
        }


        public void ToggleCamera(bool turnOn)
        {
            if (_videoSource == null) return;

            if (turnOn && !_videoSource.IsRunning)
            {
                _videoSource.Start();
            }
            else if (!turnOn && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
            }
        }

        public void ToggleMic(string targetIP, bool turnOn)
        {
            if (turnOn)
            {
                if (_waveIn == null) StartAudioCapture(targetIP);
            }
            else
            {
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
            }
        }

    }
}
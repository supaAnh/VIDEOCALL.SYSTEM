using COMMON.DTO;
using COMMON.Security;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace CLIENT.Logic
{
    public class FileProcess
    {
        private ClientSocketConnect _client;

        public FileProcess(ClientSocketConnect client)
        {
            _client = client;
        }

        public void ExecuteSendFile(string targetIP, string filePath)
        {
            if (string.IsNullOrEmpty(targetIP) || !File.Exists(filePath)) return;

            try
            {
                var fileDto = new FilePackageDTO
                {
                    FileName = Path.GetFileName(filePath),
                    FileData = File.ReadAllBytes(filePath)
                };

                byte[] rawData = JsonSerializer.SerializeToUtf8Bytes(fileDto);

                if (_client.AesKey != null)
                {
                    // 1. Mã hóa nội dung file
                    byte[] encrypted = COMMON.Security.AES_Service.Encrypt(rawData, _client.AesKey);

                    // 2. Chuẩn bị Header: [Độ dài IP (4B)][Chuỗi IP][Dữ liệu mã hóa]
                    byte[] ipBytes = Encoding.UTF8.GetBytes(targetIP);
                    byte[] ipLengthBytes = BitConverter.GetBytes(ipBytes.Length);

                    byte[] finalPayload = new byte[4 + ipBytes.Length + encrypted.Length];

                    Buffer.BlockCopy(ipLengthBytes, 0, finalPayload, 0, 4);
                    Buffer.BlockCopy(ipBytes, 0, finalPayload, 4, ipBytes.Length);
                    Buffer.BlockCopy(encrypted, 0, finalPayload, 4 + ipBytes.Length, encrypted.Length);

                    _client.Send(new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.SendFile, finalPayload).Pack());
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi gửi file: " + ex.Message); }
        }


        public string ProcessIncomingFile(byte[] encryptedData)
        {
            try
            {
                if (_client.AesKey == null) return null;

                byte[] decrypted = COMMON.Security.AES_Service.Decrypt(encryptedData, _client.AesKey);
                var fileDto = JsonSerializer.Deserialize<FilePackageDTO>(decrypted);

                if (fileDto != null)
                {
                    // Cách lấy thư mục Downloads an toàn hơn
                    string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(downloadsPath)) Directory.CreateDirectory(downloadsPath);

                    string filePath = Path.Combine(downloadsPath, fileDto.FileName);

                    // Ghi file
                    File.WriteAllBytes(filePath, fileDto.FileData);

                    // In đường dẫn ra Console để bạn kiểm tra chính xác file nằm đâu
                    Console.WriteLine("File saved at: " + filePath);

                    // Mở thư mục và chọn file
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");

                    return fileDto.FileName;
                }
            }
            catch (Exception ex)
            {
                // Thay vì Console.WriteLine, hãy dùng MessageBox để thấy lỗi thực sự khi đang chạy
                MessageBox.Show("Lỗi ghi file: " + ex.Message);
            }
            return null;
        }
    }
}
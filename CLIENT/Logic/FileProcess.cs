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
                // Tạo đối tượng DTO giống cách bài Remote làm
                var fileDto = new FilePackageDTO
                {
                    FileName = Path.GetFileName(filePath),
                    FileData = File.ReadAllBytes(filePath)
                };

                // Chuyển đối tượng thành mảng byte (Serialize)
                byte[] rawData = JsonSerializer.SerializeToUtf8Bytes(fileDto);

                if (_client.AesKey != null)
                {
                    // Mã hóa nội dung bằng AES
                    byte[] encrypted = COMMON.Security.AES_Service.Encrypt(rawData, _client.AesKey);

                    // Tạo Header IP đích không mã hóa để Server điều hướng
                    string header = targetIP + ":";
                    byte[] headerBytes = Encoding.UTF8.GetBytes(header);
                    byte[] finalPayload = new byte[headerBytes.Length + encrypted.Length];

                    Buffer.BlockCopy(headerBytes, 0, finalPayload, 0, headerBytes.Length);
                    Buffer.BlockCopy(encrypted, 0, finalPayload, headerBytes.Length, encrypted.Length);

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

                // 1. Giải mã dữ liệu
                byte[] decrypted = COMMON.Security.AES_Service.Decrypt(encryptedData, _client.AesKey);

                // 2. Chuyển byte[] ngược lại thành đối tượng DTO (Deserialize)
                var fileDto = JsonSerializer.Deserialize<FilePackageDTO>(decrypted);

                if (fileDto != null)
                {
                    string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    string filePath = Path.Combine(downloadsPath, fileDto.FileName);

                    File.WriteAllBytes(filePath, fileDto.FileData);

                    // 3. Tự động mở thư mục và chọn file (Tham khảo từ RemoteDesktop)
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");

                    return fileDto.FileName;
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi nhận file: " + ex.Message); }
            return null;
        }
    }
}
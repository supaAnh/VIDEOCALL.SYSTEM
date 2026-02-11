using System.IO;
using COMMON.DTO;
using COMMON.Security;

namespace CLIENT.Process
{
    public class FileLogic
    {
        private ClientSocketConnect _client;

        public FileLogic(ClientSocketConnect client)
        {
            _client = client;
        }

        // Hàm xử lý gửi file
        public void SendFile(string targetIP, string filePath)
        {
            if (_client.AesKey == null) return;

            string fileName = Path.GetFileName(filePath);
            byte[] fileData = File.ReadAllBytes(filePath);

            // Tạo chuỗi nội dung gồm IP đích, tên file và dữ liệu file
            string rawContent = $"{targetIP}|{fileName}|{Convert.ToBase64String(fileData)}";

            // Mã hóa toàn bộ nội dung bằng AES
            byte[] encrypted = AES_Service.EncryptString(rawContent, _client.AesKey);

            var package = new DataPackage(PackageType.SendFile, encrypted);
            _client.Send(package.Pack());
        }

        // Hàm xử lý lưu file khi nhận được
        public string SaveIncomingFile(string fileName, byte[] data)
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string fullPath = Path.Combine(downloadsPath, fileName);

            // Xử lý nếu trùng tên file
            int count = 1;
            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            while (File.Exists(fullPath))
            {
                fullPath = Path.Combine(downloadsPath, $"{fileNameOnly}({count++}){extension}");
            }

            File.WriteAllBytes(fullPath, data);
            return Path.GetFileName(fullPath);
        }



    }
}
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using COMMON.DTO;
using COMMON.Security;

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
            if (string.IsNullOrEmpty(targetIP)) return;

            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);

                // Định dạng: TenFile|FileData
                byte[] nameBytes = Encoding.UTF8.GetBytes(fileName + "|");
                byte[] combined = new byte[nameBytes.Length + fileData.Length];
                Buffer.BlockCopy(nameBytes, 0, combined, 0, nameBytes.Length);
                Buffer.BlockCopy(fileData, 0, combined, nameBytes.Length, fileData.Length);

                if (_client.AesKey != null)
                {
                    byte[] encrypted = AES_Service.Encrypt(combined, _client.AesKey);

                    // Gói IP đích vào header để Server điều hướng
                    string header = targetIP + ":";
                    byte[] headerBytes = Encoding.UTF8.GetBytes(header);
                    byte[] finalPayload = new byte[headerBytes.Length + encrypted.Length];
                    Buffer.BlockCopy(headerBytes, 0, finalPayload, 0, headerBytes.Length);
                    Buffer.BlockCopy(encrypted, 0, finalPayload, headerBytes.Length, encrypted.Length);

                    _client.Send(new DataPackage(PackageType.SendFile, finalPayload).Pack());
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi đọc file: " + ex.Message); }
        }

        public string ProcessIncomingFile(byte[] encryptedData)
        {
            // Giải mã bằng khóa AES
            byte[] decrypted = AES_Service.Decrypt(encryptedData, _client.AesKey);

            int separatorIndex = Array.IndexOf(decrypted, (byte)'|');
            if (separatorIndex != -1)
            {
                string fileName = Encoding.UTF8.GetString(decrypted, 0, separatorIndex);
                byte[] fileBytes = new byte[decrypted.Length - separatorIndex - 1];
                Buffer.BlockCopy(decrypted, separatorIndex + 1, fileBytes, 0, fileBytes.Length);

                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string path = Path.Combine(downloadsPath, fileName);
                File.WriteAllBytes(path, fileBytes);

                // KHẮC PHỤC LỖI CS0103/CS0234: Dùng đường dẫn đầy đủ của System.Diagnostics
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");

                return fileName;
            }
            return null;
        }
    }
}
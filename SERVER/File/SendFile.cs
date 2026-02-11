using COMMON.DTO;
using COMMON.Security;
using SERVER.Database;
using SERVER.LogUI;
using System.Net.Sockets;
using System.Text;

namespace SERVER.File
{
    public class SendFile
    {


        public void ForwardFile(Socket senderSocket, COMMON.DTO.DataPackage package, Dictionary<Socket, byte[]> clientKeys)
        {
            try
            {
                byte[] fullContent = package.Content;
                if (fullContent.Length < 4) return;

                // 1. Đọc độ dài IP mục tiêu
                int ipLength = BitConverter.ToInt32(fullContent, 0);

                // 2. Trích xuất IP mục tiêu
                string targetIP = Encoding.UTF8.GetString(fullContent, 4, ipLength);

                // 3. Trích xuất phần dữ liệu file đã mã hóa
                int headerSize = 4 + ipLength;
                byte[] fileEncrypted = new byte[fullContent.Length - headerSize];
                Buffer.BlockCopy(fullContent, headerSize, fileEncrypted, 0, fileEncrypted.Length);

                lock (clientKeys)
                {
                    foreach (var item in clientKeys)
                    {
                        if (item.Key.RemoteEndPoint.ToString() == targetIP && item.Key.Connected)
                        {
                            // GIẢI MÃ bằng khóa của người gửi
                            byte[] decryptedRaw = COMMON.Security.AES_Service.Decrypt(fileEncrypted, clientKeys[senderSocket]);

                            // MÃ HÓA LẠI bằng khóa của người nhận
                            byte[] reEncrypted = COMMON.Security.AES_Service.Encrypt(decryptedRaw, item.Value);

                            item.Key.Send(new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.SendFile, reEncrypted).Pack());
                            LogViewUI.AddLog($"Chuyển tiếp file: {senderSocket.RemoteEndPoint} -> {targetIP}");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) { LogViewUI.AddLog("Lỗi chuyển tiếp file: " + ex.Message); }
        }
    }
}
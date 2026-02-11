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
                int colonIdx = Array.IndexOf(fullContent, (byte)':');

                if (colonIdx != -1)
                {
                    string targetIP = Encoding.UTF8.GetString(fullContent, 0, colonIdx);
                    byte[] fileEncrypted = new byte[fullContent.Length - colonIdx - 1];
                    Buffer.BlockCopy(fullContent, colonIdx + 1, fileEncrypted, 0, fileEncrypted.Length);

                    lock (clientKeys)
                    {
                        // Tìm đúng Client nhận dựa trên IP
                        foreach (var item in clientKeys)
                        {
                            if (item.Key.RemoteEndPoint.ToString() == targetIP && item.Key.Connected)
                            {
                                // Giải mã bằng khóa người gửi và mã hóa lại bằng khóa người nhận
                                byte[] decrypted = COMMON.Security.AES_Service.Decrypt(fileEncrypted, clientKeys[senderSocket]);
                                byte[] reEncrypted = COMMON.Security.AES_Service.Encrypt(decrypted, item.Value);

                                item.Key.Send(new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.SendFile, reEncrypted).Pack());
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog("Lỗi chuyển tiếp: " + ex.Message); }
        }
    }
}
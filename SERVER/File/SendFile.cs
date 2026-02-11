using System.Net.Sockets;
using System.Text;
using COMMON.DTO;
using COMMON.Security;
using SERVER.Database;

namespace SERVER.File
{
    public class SendFile
    {
        private DatabaseConnect _db = new DatabaseConnect();


        public void ForwardFile(Socket senderSocket, DataPackage package, Dictionary<Socket, byte[]> clientKeys)
        {
            try
            {
                byte[] fullContent = package.Content;
                // 1. Tìm vị trí dấu ':' phân tách IP đích
                int colonIdx = -1;
                for (int i = 0; i < fullContent.Length; i++)
                {
                    if (fullContent[i] == (byte)':') { colonIdx = i; break; }
                }

                if (colonIdx != -1)
                {
                    // 2. Tách IP đích (Dạng chuỗi)
                    string targetIP = Encoding.UTF8.GetString(fullContent, 0, colonIdx);

                    // 3. Tách dữ liệu file đã mã hóa (Dạng mảng byte gốc)
                    byte[] fileEncrypted = new byte[fullContent.Length - colonIdx - 1];
                    Buffer.BlockCopy(fullContent, colonIdx + 1, fileEncrypted, 0, fileEncrypted.Length);

                    string senderIP = senderSocket.RemoteEndPoint.ToString();
                    _db.SaveChatMessage(senderIP, targetIP, "[Gửi file]");

                    lock (clientKeys)
                    {
                        foreach (var item in clientKeys)
                        {
                            if (item.Key.RemoteEndPoint.ToString() == targetIP && item.Key.Connected)
                            {
                                // 4. Giải mã bằng khóa của người gửi và mã hóa lại bằng khóa người nhận
                                byte[] decrypted = AES_Service.Decrypt(fileEncrypted, clientKeys[senderSocket]);
                                byte[] reEncrypted = AES_Service.Encrypt(decrypted, item.Value);

                                item.Key.Send(new DataPackage(PackageType.SendFile, reEncrypted).Pack());
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog("Lỗi chuyển tiếp file: " + ex.Message); }
        }
    }
}
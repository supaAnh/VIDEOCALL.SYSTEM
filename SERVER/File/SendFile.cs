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
            string senderIP = senderSocket.RemoteEndPoint.ToString();
            string rawData = Encoding.UTF8.GetString(package.Content);
            int colonIdx = rawData.IndexOf(':');

            if (colonIdx != -1)
            {
                string targetIP = rawData.Substring(0, colonIdx);
                byte[] fileEncrypted = package.Content.Skip(colonIdx + 1).ToArray();

                // Lưu log vào database
                _db.SaveChatMessage(senderIP, targetIP, "[Gửi file thành công]");

                lock (clientKeys)
                {
                    foreach (var item in clientKeys)
                    {
                        if (item.Key.RemoteEndPoint.ToString() == targetIP && item.Key.Connected)
                        {
                            // Giải mã khóa sender, mã hóa lại khóa receiver
                            byte[] decrypted = AES_Service.Decrypt(fileEncrypted, clientKeys[senderSocket]);
                            byte[] reEncrypted = AES_Service.Encrypt(decrypted, item.Value);

                            item.Key.Send(new DataPackage(PackageType.SendFile, reEncrypted).Pack());
                            break;
                        }
                    }
                }
            }
        }
    }
}
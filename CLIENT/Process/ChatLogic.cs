// CLIENT/Process/ChatLogic.cs
using COMMON.DTO;
using COMMON.Security;
using System.Text;

namespace CLIENT.Process
{
    public class ChatLogic
    {
        private ClientSocketConnect _client;

        public ChatLogic(ClientSocketConnect client)
        {
            _client = client;
        }

        public void SendPrivateMessage(string targetIP, string message)
        {
            if (_client.AesKey == null) return;

            //Tạo đối tượng chứa dữ liệu chat riêng
            DataObject obj = new DataObject
            {
                ReceiverName = targetIP,
                Data = Encoding.UTF8.GetBytes(message),
                Type = MessageType.Message
            };

            // Chuyển object thành mảng byte và mã hóa
            byte[] encryptedMsg = AES_Service.Encrypt(obj.Data, _client.AesKey);

            // Đóng gói vào DataPackage với metadata người nhận
            string rawContent = $"{targetIP}|{Convert.ToBase64String(encryptedMsg)}";
            byte[] finalContent = Encoding.UTF8.GetBytes(rawContent);

            var package = new DataPackage(PackageType.SecureMessage, finalContent);
            _client.Send(package.Pack());
        }
    }
}

using COMMON.DTO;
using COMMON.Security;
using System.Text;

namespace CLIENT.Logic
{
    public class ChatProcess
    {
        private ClientSocketConnect _client;

        public ChatProcess(ClientSocketConnect client)
        {
            _client = client;
        }

        public void SendPrivateMessage(string targetIP, string message)
        {
            if (_client.AesKey == null) return;

            // Định dạng gói tin: "IP_Nguoi_Nhan|Noi_Dung_Tin_Nhan"
            // Việc ghép chuỗi này giúp Server biết cần gửi cho ai mà không cần giải mã tin nhắn
            string rawData = $"{targetIP}|{message}";

            // Mã hóa toàn bộ chuỗi trên bằng AES
            byte[] encryptedContent = AES_Service.EncryptString(rawData, _client.AesKey);

            // Đóng gói vào DataPackage loại SecureMessage (Loại 7)
            var package = new DataPackage(PackageType.SecureMessage, encryptedContent);

            _client.Send(package.Pack());
        }
    }
}
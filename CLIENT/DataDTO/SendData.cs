using System.Net.Sockets;
using System.Text;
using COMMON.DTO;

void SendChatMessage(string message)
{
    ClientSocketConnect myClient = new ClientSocketConnect();
    // 1. Chuyển string thành mảng byte
    byte[] content = Encoding.UTF8.GetBytes(message);

    // 2. Tạo đối tượng DataPackage (loại ChatMessage = 1)
    var package = new DataPackage(PackageType.ChatMessage, content);

    // 3. Đóng gói thành mảng byte
    byte[] finalData = package.Pack();

    // 4. Gửi qua Socket
    myClient.Send(finalData);
    
}
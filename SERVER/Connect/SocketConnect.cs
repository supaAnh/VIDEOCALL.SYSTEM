using COMMON.DTO;
using COMMON.Security;
using SERVER.LogUI;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;

public class SocketConnect
{
    //Địa chỉ IP và cổng của server
    IPEndPoint IP;
    Socket server;
    List<Socket> clientList;


    //
    // Khởi động Server
    //
    public void StartServer(int port)
    {
        clientList = new List<Socket>();
        IP = new IPEndPoint(IPAddress.Any, port);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            server.Bind(IP);
            server.Listen(100); // Bắt đầu lắng nghe tại đây

            // Khởi chạy luồng để chấp nhận các kết nối
            Thread listenThread = new Thread(AcceptConnections);
            listenThread.IsBackground = true;
            listenThread.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể khởi động Server: " + ex.Message);
        }
    }


    //
    // Chấp nhận kết nối từ Client
    //
    private void AcceptConnections()
    {
        try
        {
            while (true)
            {
                // Chấp nhận kết nối mới
                Socket client = server.Accept();
                clientList.Add(client);

                // --- GỬI KEY NGAY KHI CHẤP NHẬN KẾT NỐI ---
                SendEncryptionKey(client);

                // Tạo luồng nhận dữ liệu cho client này
                Thread receive = new Thread(ReceiveData);
                receive.IsBackground = true;
                receive.Start(client);

                // Cập nhật giao diện
                string clientIP = client.RemoteEndPoint.ToString();
                LogViewUI.AddClient(clientIP);
                LogViewUI.AddLog($" [{clientIP}]: đã kết nối và nhận khóa bảo mật");
            }
        }
        catch (Exception ex)
        {
            // Xử lý khi server dừng hoặc lỗi
            LogViewUI.AddLog("Lỗi trong quá trình Accept: " + ex.Message);
        }
    }

    //
    // Gửi khóa AES cho Client
    //

    private void SendEncryptionKey(Socket client)
    {
        // Khóa AES cố định (nên trùng với khóa bạn dự định dùng ở Client)
        byte[] randomKey = CreateKey.GenerateRandomKey();

        // Đóng gói vào DataPackage loại DH_KeyExchange (Loại 6)
        DataPackage keyPackage = new DataPackage(PackageType.DH_KeyExchange, randomKey);

        // Gửi mảng byte đã đóng gói
        client.Send(keyPackage.Pack());
    }


    //
    //Hàm nhận data từ client
    //
    public void ReceiveData(object obj)
    {
        Socket client = obj as Socket;
        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024 * 5000];
                int received = client.Receive(buffer);
                if (received == 0) break;

                // Giải gói tin để biết loại dữ liệu
                byte[] actualData = new byte[received];
                Array.Copy(buffer, 0, actualData, 0, received);
                DataPackage package = DataPackage.Unpack(actualData);

                if (package.Type == PackageType.ChatMessage)
                {
                    // Log tin nhắn lên giao diện Server
                    string msg = Encoding.UTF8.GetString(package.Content);
                    SERVER.LogUI.LogViewUI.AddLog($"Tin nhắn từ [{client.RemoteEndPoint}]: {msg}");

                    // Chuyển tiếp cho các Client khác
                    foreach (Socket item in clientList)
                    {
                        if (item != null && item != client && item.Connected)
                            item.Send(actualData);
                    }
                }
            }
        }
        catch
        {
            clientList.Remove(client);
            client.Close();
        }
    }

    // Tạo key cho AES từng CLIENT
    
}

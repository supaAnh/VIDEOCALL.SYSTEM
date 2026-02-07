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
    // Trong SocketConnect.cs
    private Dictionary<Socket, byte[]> clientKeys = new Dictionary<Socket, byte[]>();

    // Kết nối Database
    private SERVER.Database.DatabaseConnect db = new SERVER.Database.DatabaseConnect();
    //
    // Khởi động Server
    //
    public void StartServer(int port)
    {
        // KIỂM TRA KẾT NỐI DATABASE NGAY KHI KHỞI TẠO
        if (db.TestConnection())
        {
            LogViewUI.AddLog("Database đã kết nối thành công (Port 1555).");
        }
        else
        {
            LogViewUI.AddLog("Không thể kết nối Database!");
        }


        // Khởi tạo dictionary lưu trữ khóa AES cho từng Client
        clientKeys = new Dictionary<Socket, byte[]>();
        // Thiết lập địa chỉ IP và cổng
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
                Socket client = server.Accept();

                // 1. Gửi Key AES trước
                SendEncryptionKey(client);

                // 2. Bắt đầu luồng nhận dữ liệu
                Thread receive = new Thread(ReceiveData);
                receive.IsBackground = true;
                receive.Start(client);

                // 3. Cập nhật UI trên Server
                string clientIP = client.RemoteEndPoint.ToString();
                LogViewUI.AddClient(clientIP);
                LogViewUI.AddLog($" [{clientIP}]: đã kết nối");

                // QUAN TRỌNG: Đợi 1 chút để Client khởi tạo Form Main xong
                // và gọi Broadcast cho TẤT CẢ mọi người trong danh sách
                Thread.Sleep(300);
                BroadcastOnlineList();
            }
        }
        catch (Exception ex)
        {
            // Xử lý khi server dừng hoặc lỗi
            LogViewUI.AddLog("Lỗi trong quá trình Accept: " + ex.Message);
            BroadcastOnlineList();
        }
    }

    //
    // Gửi khóa AES cho Client
    //

    private void SendEncryptionKey(Socket client)
    {
        // Khóa AES cố định (nên trùng với khóa bạn dự định dùng ở Client)
        byte[] randomKey = CreateKey.GenerateRandomKey();

        lock (clientKeys)
        {
            clientKeys[client] = randomKey;
        }
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
                // Lấy thông tin IP của người gửi
                string senderIP = client.RemoteEndPoint.ToString();

                if (package.Type == PackageType.UserStatusUpdate)
                {
                    BroadcastOnlineList();
                }
                else if (package.Type == PackageType.ChatMessage || package.Type == PackageType.SecureMessage)
                {
                    string displayMsg = "";
                    if (package.Type == PackageType.SecureMessage)
                    {
                        // Tìm khóa AES tương ứng với Socket đang gửi tin nhắn
                        if (clientKeys.TryGetValue(client, out byte[] key))
                        {
                            try
                            {
                                string decrypted = AES_Service.DecryptString(package.Content, key);
                                if (decrypted.Contains("|"))
                                {
                                    string targetIP = decrypted.Split('|')[0];
                                    string messageContent = decrypted.Split('|')[1];

                                    LogViewUI.AddLog($"Chat riêng: [{client.RemoteEndPoint}] -> [{targetIP}]: {messageContent}");

                                    // Chuyển tiếp nguyên gói tin (đã đóng gói) cho người nhận
                                    SERVER.Process.MessageDispatcher.ForwardToTarget(targetIP, actualData, clientKeys);
                                    db.SaveChatMessage(senderIP, targetIP, messageContent);
                                }
                            }
                            catch
                            {
                                displayMsg = "[Lỗi giải mã]";
                            }
                        }
                        else
                        {
                            displayMsg = "[Không tìm thấy khóa bảo mật]";
                        }
                    }
                    else if (package.Type == PackageType.ChatMessage)
                    {
                        displayMsg = Encoding.UTF8.GetString(package.Content); 
                    }

                    SERVER.LogUI.LogViewUI.AddLog($"Tin nhắn từ [{client.RemoteEndPoint}]: {displayMsg}");
                    // Chuyển tiếp cho các Client khác
                    foreach (Socket item in clientKeys.Keys)
                    {
                        if (item != null && item != client && item.Connected)
                        {
                            item.Send(actualData);
                        }
                    }
                }
            }
        }
        catch
        {
            lock (clientKeys)
            {
                if (clientKeys.ContainsKey(client))
                {
                    clientKeys.Remove(client);
                }
            }
            client.Close();
        }
    }

    //
    // Phát danh sách online cho tất cả client
    //
    private void BroadcastOnlineList()
    {
        List<string> onlineUsers = new List<string>();

        // Khóa lock để tránh lỗi khi nhiều luồng truy cập dictionary cùng lúc
        lock (clientKeys)
        {
            foreach (var s in clientKeys.Keys)
            {
                if (s != null && s.Connected)
                {
                    onlineUsers.Add(s.RemoteEndPoint.ToString());
                }
            }
        }

        string listString = string.Join(",", onlineUsers);
        byte[] content = Encoding.UTF8.GetBytes(listString);

        // Sử dụng PackageType.UserStatusUpdate (Loại 5)
        DataPackage listPackage = new DataPackage(PackageType.UserStatusUpdate, content);
        byte[] finalData = listPackage.Pack();

        lock (clientKeys)
        {
            foreach (var client in clientKeys.Keys)
            {
                if (client != null && client.Connected)
                {
                    try { client.Send(finalData); } catch { }
                }
            }
        }
    }

}

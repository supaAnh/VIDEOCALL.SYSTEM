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
        Socket senderSocket = obj as Socket;
        if (senderSocket == null) return;

        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024 * 5000];
                int received = senderSocket.Receive(buffer);
                if (received == 0) break;

                byte[] actualData = new byte[received];
                Array.Copy(buffer, 0, actualData, 0, received);

                // Giải gói tin thô để lấy nội dung (đã mã hóa)
                DataPackage package = DataPackage.Unpack(actualData);
                string senderIP = senderSocket.RemoteEndPoint.ToString();

                if (package.Type == PackageType.UserStatusUpdate)
                {
                    BroadcastOnlineList();
                }
                else if (package.Type == PackageType.SecureMessage)
                {
                    // Lấy Key của người gửi để giải mã tin nhắn
                    if (clientKeys.TryGetValue(senderSocket, out byte[] senderKey))
                    {
                        try
                        {
                            // Giải mã để đọc thông tin targetIP và nội dung tin nhắn
                            string decrypted = AES_Service.DecryptString(package.Content, senderKey);

                            if (decrypted.Contains("|"))
                            {
                                string[] parts = decrypted.Split('|');
                                string targetIP = parts[0];
                                string messageContent = parts[1];

                                LogViewUI.AddLog($"Chat: [{senderIP}] -> [{targetIP}]: {messageContent}");
                                db.SaveChatMessage(senderIP, targetIP, messageContent);

                                // Tìm Socket và Key của người nhận để mã hóa lại
                                bool sent = false;
                                lock (clientKeys)
                                {
                                    foreach (var item in clientKeys)
                                    {
                                        Socket targetSocket = item.Key;
                                        byte[] targetKey = item.Value;

                                        if (targetSocket.RemoteEndPoint.ToString() == targetIP && targetSocket.Connected)
                                        {

                                            string messageToForward = $"{senderIP}|{messageContent}";

                                            // Mã hóa lại bằng Key của người nhận
                                            byte[] reEncryptedContent = AES_Service.EncryptString(messageToForward, targetKey);
                                            DataPackage forwardPackage = new DataPackage(PackageType.SecureMessage, reEncryptedContent);

                                            targetSocket.Send(forwardPackage.Pack());
                                            sent = true;
                                            break;
                                        }
                                    }
                                }

                                if (!sent)
                                {
                                    LogViewUI.AddLog($" !!!Không tìm thấy người nhận: {targetIP}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogViewUI.AddLog($" !!!Lỗi xử lý tin nhắn bảo mật: {ex.Message}");
                        }
                    }
                    else
                    {
                        LogViewUI.AddLog($" !!!Không tìm thấy khóa của sender: {senderIP}");
                    }
                }
                else if (package.Type == PackageType.ChatMessage)
                {
                    string msg = Encoding.UTF8.GetString(package.Content);
                    LogViewUI.AddLog($"Tin nhắn thường từ [{senderIP}]: {msg}");

                    // Broadcast cho các client khác (không mã hóa)
                    lock (clientKeys)
                    {
                        foreach (Socket client in clientKeys.Keys)
                        {
                            if (client != senderSocket && client.Connected)
                            {
                                client.Send(actualData);
                            }
                        }
                    }
                }
                else if (package.Type == PackageType.RequestChatHistory)
                {
                    string targetIP = Encoding.UTF8.GetString(package.Content);
                    string requesterIP = senderSocket.RemoteEndPoint.ToString();

                    // Lấy từ DB
                    List<string> logs = db.GetChatHistory(requesterIP, targetIP);

                    foreach (string line in logs)
                    {
                        // Gửi trả lại cho người yêu cầu dưới dạng SecureMessage để Client tự hiển thị
                        // Lưu ý: Bạn cần mã hóa lại tin nhắn này bằng Key của người yêu cầu
                        if (clientKeys.TryGetValue(senderSocket, out byte[] key))
                        {
                            byte[] encrypted = AES_Service.EncryptString(line, key);
                            DataPackage historyPkg = new DataPackage(PackageType.SecureMessage, encrypted);
                            senderSocket.Send(historyPkg.Pack());
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogViewUI.AddLog($"Client [{senderSocket.RemoteEndPoint}] ngắt kết nối: {ex.Message}");
        }
        finally
        {
            HandleDisconnect(senderSocket);
        }
    }

    // hàm xử lý ngắt kết nối 
    private void HandleDisconnect(Socket client)
    {
        lock (clientKeys)
        {
            if (clientKeys.ContainsKey(client))
            {
                clientKeys.Remove(client);
            }
        }
        string ip = client.RemoteEndPoint?.ToString() ?? "Unknown";
        client.Close();
        LogViewUI.RemoveClient(ip);
        BroadcastOnlineList();
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

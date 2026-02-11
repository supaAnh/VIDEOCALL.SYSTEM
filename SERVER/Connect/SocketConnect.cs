using COMMON.DTO;
using COMMON.Security;
using SERVER.LogUI;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;

public class SocketConnect
{
    //Địa chỉ IP và cổng của server
    IPEndPoint IP;
    Socket server;

    // Lưu trữ khóa AES cho từng Client
    private Dictionary<Socket, byte[]> clientKeys = new Dictionary<Socket, byte[]>();

    // Lưu trữ thông tin nhóm và thành viên
    private Dictionary<string, List<string>> groupMembersTable = new Dictionary<string, List<string>>();

    // Kết nối Database
    private SERVER.Database.DatabaseConnect db = new SERVER.Database.DatabaseConnect();

    // Xử lý file
    private SERVER.File.SendFile _fileHandler = new SERVER.File.SendFile();

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

    public void ReceiveData(object obj)
    {
        Socket senderSocket = obj as Socket;
        if (senderSocket == null) return;

        // Bộ đệm tích lũy dữ liệu thừa từ lần nhận trước (Xử lý dính gói/cắt gói)
        byte[] leftOver = new byte[0];

        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024 * 5000]; // 5MB
                int received = senderSocket.Receive(buffer);
                if (received == 0) break;

                // 1. Kết hợp dữ liệu dư thừa cũ với dữ liệu vừa nhận được
                byte[] dataToProcess = new byte[leftOver.Length + received];
                Buffer.BlockCopy(leftOver, 0, dataToProcess, 0, leftOver.Length);
                Buffer.BlockCopy(buffer, 0, dataToProcess, leftOver.Length, received);

                int offset = 0;
                // 2. Vòng lặp tách từng gói tin trong mảng byte tích lũy
                // Cấu trúc gói: 1 byte Type + 4 byte Length + N byte Content (Tối thiểu 5 byte)
                while (offset + 5 <= dataToProcess.Length)
                {
                    // Đọc độ dài Content từ byte thứ 2 đến thứ 5
                    int payloadLength = BitConverter.ToInt32(dataToProcess, offset + 1);
                    int totalPacketSize = 5 + payloadLength;

                    // Kiểm tra xem đã nhận đủ toàn bộ gói tin chưa
                    if (dataToProcess.Length >= offset + totalPacketSize)
                    {
                        // Tách đúng một gói tin hoàn chỉnh
                        byte[] packetBytes = new byte[totalPacketSize];
                        Buffer.BlockCopy(dataToProcess, offset, packetBytes, 0, totalPacketSize);

                        // Giải gói và xử lý logic
                        DataPackage package = DataPackage.Unpack(packetBytes);
                        HandlePackage(senderSocket, package); // Gọi hàm xử lý riêng bên dưới

                        offset += totalPacketSize; // Di chuyển vị trí đọc sang gói tiếp theo
                    }
                    else
                    {
                        // Chưa nhận đủ gói tin hoàn chỉnh (bị cắt đôi), thoát vòng lặp để đợi Receive tiếp
                        break;
                    }
                }

                // 3. Lưu lại phần dữ liệu còn dư (nếu có) vào leftOver cho lần nhận sau
                int remaining = dataToProcess.Length - offset;
                leftOver = new byte[remaining];
                if (remaining > 0)
                {
                    Buffer.BlockCopy(dataToProcess, offset, leftOver, 0, remaining);
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

    //
    //Hàm nhận data từ client
    //
    // Hàm xử lý logic tập trung cho từng gói tin sau khi đã được tách đúng
    private void HandlePackage(Socket senderSocket, DataPackage package)
    {
        string senderIP = senderSocket.RemoteEndPoint.ToString();

        switch (package.Type)
        {
            // 1. Cập nhật danh sách Online
            case PackageType.UserStatusUpdate:
                BroadcastOnlineList();
                break;

            // 2. Tin nhắn bảo mật (Chat riêng)
            case PackageType.SecureMessage:
                if (clientKeys.TryGetValue(senderSocket, out byte[] senderKey))
                {
                    try
                    {
                        string decrypted = AES_Service.DecryptString(package.Content, senderKey);
                        if (decrypted.Contains("|"))
                        {
                            string[] parts = decrypted.Split('|');
                            string targetIP = parts[0];
                            string messageContent = parts[1];

                            LogViewUI.AddLog($"Chat: [{senderIP}] -> [{targetIP}]: {messageContent}");
                            db.SaveChatMessage(senderIP, targetIP, messageContent);

                            ForwardToClient(targetIP, senderIP, messageContent, PackageType.SecureMessage);
                        }
                    }
                    catch (Exception ex) { LogViewUI.AddLog($"!!! Lỗi SecureMessage: {ex.Message}"); }
                }
                break;

            // 3. Tin nhắn thường (Không mã hóa)
            case PackageType.ChatMessage:
                string msg = Encoding.UTF8.GetString(package.Content);
                LogViewUI.AddLog($"Tin nhắn thường từ [{senderIP}]: {msg}");

                lock (clientKeys)
                {
                    foreach (Socket client in clientKeys.Keys)
                    {
                        if (client != senderSocket && client.Connected)
                        {
                            client.Send(package.Pack());
                        }
                    }
                }
                break;

            // 4. Tạo nhóm mới
            case PackageType.CreateGroup:
                string groupData = Encoding.UTF8.GetString(package.Content);
                string[] gParts = groupData.Split('|');
                string newGroupName = gParts[0];
                List<string> members = gParts[1].Split(',').ToList();

                if (!members.Contains(senderIP)) members.Add(senderIP);

                lock (groupMembersTable)
                {
                    groupMembersTable[newGroupName] = members;
                }

                // Thông báo cho tất cả thành viên trong nhóm
                DataPackage gUpdate = new DataPackage(PackageType.GroupUpdate, Encoding.UTF8.GetBytes(newGroupName));
                BroadcastToList(members, gUpdate.Pack());
                break;

            // 5. Tin nhắn nhóm
            case PackageType.GroupMessage:
                if (clientKeys.TryGetValue(senderSocket, out byte[] sKey))
                {
                    try
                    {
                        string dec = AES_Service.DecryptString(package.Content, sKey);
                        string[] p = dec.Split('|');
                        if (p.Length < 2) return;

                        string gName = p[0];
                        string gContent = p[1];

                        LogViewUI.AddLog($"Group [{gName}]: {senderIP} -> {gContent}");
                        db.SaveGroupMessage(senderIP, gName, gContent);

                        if (groupMembersTable.ContainsKey(gName))
                        {
                            List<string> gMembers = groupMembersTable[gName];
                            string forwardData = $"{gName}|{senderIP}|{gContent}";
                            BroadcastToGroup(gMembers, forwardData, senderSocket);
                        }
                    }
                    catch (Exception ex) { LogViewUI.AddLog($"!!! Lỗi GroupMsg: {ex.Message}"); }
                }
                break;

            // 6. Gửi file
            case PackageType.SendFile:
                _fileHandler.ForwardFile(senderSocket, package, clientKeys);
                break;

            // 7. Yêu cầu lịch sử Chat
            case PackageType.RequestChatHistory:
                string target = Encoding.UTF8.GetString(package.Content);
                List<string> logs = groupMembersTable.ContainsKey(target)
                                    ? db.GetGroupChatHistory(target)
                                    : db.GetChatHistory(senderIP, target);

                foreach (string line in logs)
                {
                    if (clientKeys.TryGetValue(senderSocket, out byte[] k))
                    {
                        byte[] enc = AES_Service.EncryptString(line, k);
                        PackageType type = groupMembersTable.ContainsKey(target) ? PackageType.GroupMessage : PackageType.SecureMessage;
                        senderSocket.Send(new DataPackage(type, enc).Pack());
                    }
                }
                break;
        }
    }

    // Hàm bổ trợ: Chuyển tiếp tin nhắn riêng
    private void ForwardToClient(string targetIP, string senderIP, string content, PackageType type)
    {
        lock (clientKeys)
        {
            foreach (var item in clientKeys)
            {
                if (item.Key.RemoteEndPoint.ToString() == targetIP && item.Key.Connected)
                {
                    string data = (type == PackageType.SecureMessage) ? $"{senderIP}|{content}" : content;
                    byte[] enc = AES_Service.EncryptString(data, item.Value);
                    item.Key.Send(new DataPackage(type, enc).Pack());
                    return;
                }
            }
        }
    }

    // Hàm bổ trợ: Gửi cho danh sách IP cụ thể
    private void BroadcastToList(List<string> ips, byte[] data)
    {
        lock (clientKeys)
        {
            foreach (var client in clientKeys.Keys)
            {
                if (ips.Contains(client.RemoteEndPoint.ToString()) && client.Connected)
                {
                    client.Send(data);
                }
            }
        }
    }

    // Hàm bổ trợ: Gửi tin nhắn nhóm (có mã hóa riêng cho từng người)
    private void BroadcastToGroup(List<string> members, string rawData, Socket sender)
    {
        lock (clientKeys)
        {
            foreach (var item in clientKeys)
            {
                if (members.Contains(item.Key.RemoteEndPoint.ToString()) && item.Key.Connected && item.Key != sender)
                {
                    byte[] enc = AES_Service.EncryptString(rawData, item.Value);
                    item.Key.Send(new DataPackage(PackageType.GroupMessage, enc).Pack());
                }
            }
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
        lock (clientKeys)
        {
            // 1. Lấy danh sách tất cả các IP đang kết nối
            List<string> allOnlineUsers = new List<string>();
            foreach (var s in clientKeys.Keys)
            {
                if (s != null && s.Connected)
                {
                    allOnlineUsers.Add(s.RemoteEndPoint.ToString());
                }
            }

            // 2. Gửi danh sách đã lọc cho từng Client cụ thể
            foreach (var client in clientKeys.Keys)
            {
                if (client != null && client.Connected)
                {
                    try
                    {
                        string myIP = client.RemoteEndPoint.ToString();
                        // Lọc bỏ IP của chính Client này khỏi danh sách gửi cho họ
                        var filteredList = allOnlineUsers.Where(ip => ip != myIP).ToList();

                        string listString = string.Join(",", filteredList);
                        byte[] content = Encoding.UTF8.GetBytes(listString);

                        DataPackage listPackage = new DataPackage(PackageType.UserStatusUpdate, content);
                        client.Send(listPackage.Pack());
                    }
                    catch { }
                }
            }
        }
    }

}

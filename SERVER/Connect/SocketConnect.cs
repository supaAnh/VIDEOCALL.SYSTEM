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

                //
                // Xử lý cập nhật trạng thái người dùng
                //
                if (package.Type == PackageType.UserStatusUpdate)
                {
                    BroadcastOnlineList();
                }

                //
                // Xử lý tin nhắn bảo mật (mã hóa AES)
                //
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

                //
                // Xử lý tin nhắn thường (không mã hóa)
                //
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

                //
                // Xử lý tạo nhóm
                //
                else if (package.Type == PackageType.CreateGroup)
                {
                    string data = Encoding.UTF8.GetString(package.Content);
                    string[] parts = data.Split('|');
                    string groupName = parts[0];
                    List<string> members = parts[1].Split(',').ToList();

                    // Luôn thêm "người nhấn tạo" vào danh sách thành viên nhóm
                    if (!members.Contains(senderIP))
                    {
                        members.Add(senderIP);
                    }

                    lock (groupMembersTable)
                    {
                        // Lưu hoặc cập nhật danh sách thành viên vào bộ nhớ Server
                        groupMembersTable[groupName] = members;
                    }

                    // Gửi thông báo có nhóm mới cho tất cả thành viên
                    DataPackage groupUpdate = new DataPackage(PackageType.GroupUpdate, Encoding.UTF8.GetBytes(groupName));
                    byte[] finalData = groupUpdate.Pack();

                    lock (clientKeys)
                    {
                        foreach (var client in clientKeys.Keys)
                        {
                            if (members.Contains(client.RemoteEndPoint.ToString()))
                            {
                                client.Send(finalData);
                            }
                        }
                    }
                }

                //
                // Xử lý gửi file
                //
                else if (package.Type == PackageType.SendFile)
                {
                    _fileHandler.ForwardFile(senderSocket, package, clientKeys);
                }

                //
                // Xử lý tin nhắn nhóm
                //
                else if (package.Type == PackageType.GroupMessage)
                {
                    if (clientKeys.TryGetValue(senderSocket, out byte[] senderKey))
                    {
                        try
                        {
                            // 1. Giải mã tin nhắn từ người gửi: "TenNhom|NoiDung"
                            string decrypted = AES_Service.DecryptString(package.Content, senderKey);
                            string[] parts = decrypted.Split('|');
                            if (parts.Length < 2) return;

                            string groupName = parts[0];
                            string messageContent = parts[1];

                            // 2. Ghi Log và Lưu vào Database để đồng bộ lịch sử
                            LogViewUI.AddLog($"Group [{groupName}]: {senderIP} nói {messageContent}");
                            db.SaveGroupMessage(senderIP, groupName, messageContent);

                            // 3. Kiểm tra xem nhóm có tồn tại trong bộ nhớ Server không
                            if (groupMembersTable.ContainsKey(groupName))
                            {
                                List<string> members = groupMembersTable[groupName];

                                lock (clientKeys)
                                {
                                    foreach (var item in clientKeys)
                                    {
                                        Socket targetSocket = item.Key;
                                        byte[] targetKey = item.Value;
                                        string targetIP = targetSocket.RemoteEndPoint.ToString();

                                        // ĐIỀU KIỆN GỬI:
                                        // - IP mục tiêu nằm trong danh sách thành viên nhóm
                                        // - Socket đang kết nối
                                        // - KHÔNG gửi lại cho chính người gửi (vì Client đã tự hiển thị rồi)
                                        if (members.Contains(targetIP) && targetSocket.Connected && targetSocket != senderSocket)
                                        {
                                            // Định dạng gói tin gửi đi: "TenNhom|IP_NguoiGui|NoiDung"
                                            string forwardData = $"{groupName}|{senderIP}|{messageContent}";
                                            byte[] reEncrypted = AES_Service.EncryptString(forwardData, targetKey);

                                            DataPackage forwardPkg = new DataPackage(PackageType.GroupMessage, reEncrypted);
                                            targetSocket.Send(forwardPkg.Pack());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LogViewUI.AddLog($" !!! Lỗi: Không tìm thấy danh sách thành viên cho nhóm {groupName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogViewUI.AddLog($" !!! Lỗi xử lý GroupMessage: {ex.Message}");
                        }
                    }
                }

                //
                // Xử lý yêu cầu lịch sử chat nhóm
                //
                else if (package.Type == PackageType.RequestChatHistory)
                {
                    string target = Encoding.UTF8.GetString(package.Content);
                    string requesterIP = senderSocket.RemoteEndPoint.ToString();

                    List<string> logs;

                    // Kiểm tra nếu 'target' là một nhóm đang hoạt động
                    if (groupMembersTable.ContainsKey(target))
                    {
                        // Lấy từ bảng GroupChatHistory
                        logs = db.GetGroupChatHistory(target);
                    }
                    else
                    {
                        // Lấy từ bảng ChatHistory (chat riêng)
                        logs = db.GetChatHistory(requesterIP, target);
                    }

                    // Gửi dữ liệu về Client
                    foreach (string line in logs)
                    {
                        if (clientKeys.TryGetValue(senderSocket, out byte[] key))
                        {
                            byte[] encrypted = AES_Service.EncryptString(line, key);
                            // Nếu là nhóm, gửi gói tin GroupMessage để Client hiển thị đúng format
                            PackageType pType = groupMembersTable.ContainsKey(target) ? PackageType.GroupMessage : PackageType.SecureMessage;

                            DataPackage historyPkg = new DataPackage(pType, encrypted);
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

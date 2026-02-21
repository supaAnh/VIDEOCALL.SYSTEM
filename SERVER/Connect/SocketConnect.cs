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

    // Lưu trữ ánh xạ giữa Socket và tên người dùng
    private Dictionary<Socket, string> _socketToUser = new Dictionary<Socket, string>();


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
    // Sửa lại hàm HandlePackage trong SERVER/Connect/SocketConnect.cs

    private void HandlePackage(Socket senderSocket, DataPackage package)
    {
        string senderIP = senderSocket.RemoteEndPoint.ToString();

        // Lấy định danh: Nếu đã đăng nhập thì lấy Username, chưa thì lấy IP
        string senderIdentity = _socketToUser.ContainsKey(senderSocket) ? _socketToUser[senderSocket] : senderIP;

        switch (package.Type)
        {
            // 1. Cập nhật danh sách Online
            case PackageType.UserStatusUpdate:
                BroadcastOnlineList();
                break;

            // --- XỬ LÝ ĐĂNG KÝ (MỚI) ---
            case PackageType.Register:
                if (clientKeys.TryGetValue(senderSocket, out byte[] regKey))
                {
                    // Đổi tên biến để tránh trùng: regData, regParts, regMsg
                    string regData = COMMON.Security.AES_Service.DecryptString(package.Content, regKey);
                    string[] regParts = regData.Split('|'); // username|password
                    if (regParts.Length >= 2)
                    {
                        bool success = db.RegisterUser(regParts[0], regParts[1]);
                        string regMsg = success ? "OK" : "FAIL";
                        byte[] response = COMMON.Security.AES_Service.EncryptString(regMsg, regKey);
                        senderSocket.Send(new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.Register, response).Pack());
                        SERVER.LogUI.LogViewUI.AddLog($"[{senderIP}] đăng ký tài khoản [{regParts[0]}]: {regMsg}");
                    }
                }
                break;

            // --- XỬ LÝ ĐĂNG NHẬP (MỚI) ---
            case PackageType.Authenticate:
                if (clientKeys.TryGetValue(senderSocket, out byte[] authKey))
                {
                    string authData = COMMON.Security.AES_Service.DecryptString(package.Content, authKey);
                    string[] authParts = authData.Split('|'); // username|password
                    if (authParts.Length >= 2)
                    {
                        string uName = authParts[0];
                        string pass = authParts[1];

                        // 1. Kiểm tra tài khoản có đang online không
                        bool isAlreadyLoggedIn = false;
                        lock (_socketToUser)
                        {
                            if (_socketToUser.ContainsValue(uName)) isAlreadyLoggedIn = true;
                        }

                        if (isAlreadyLoggedIn)
                        {
                            // Nếu trùng -> Từ chối
                            byte[] resDuplicate = COMMON.Security.AES_Service.EncryptString("DUPLICATE", authKey);
                            senderSocket.Send(new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.LoginResponse, resDuplicate).Pack());
                            SERVER.LogUI.LogViewUI.AddLog($"[{senderIP}] bị từ chối: Tài khoản [{uName}] đang online.");
                        }
                        else
                        {
                            // 2. Kiểm tra Database
                            if (db.CheckLogin(uName, pass))
                            {
                                lock (_socketToUser)
                                {
                                    _socketToUser[senderSocket] = uName;
                                }

                                SERVER.LogUI.LogViewUI.AddLog($"[{senderIP}] đăng nhập thành công [{uName}]");

                                // --- CẬP NHẬT GIAO DIỆN SERVER (QUAN TRỌNG) ---
                                // Bước A: Xóa dòng chỉ có IP (được thêm lúc mới kết nối Socket)
                                SERVER.LogUI.LogViewUI.RemoveClient(senderIP);

                                // Bước B: Thêm dòng mới định dạng "IP - Username"
                                string displayIdentity = $"{senderIP} - {uName}";
                                SERVER.LogUI.LogViewUI.AddClient(displayIdentity);

                                // Trả về OK cho Client
                                byte[] resOK = COMMON.Security.AES_Service.EncryptString("OK", authKey);
                                senderSocket.Send(new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.LoginResponse, resOK).Pack());

                                // Cập nhật danh sách online cho mọi người
                                BroadcastOnlineList();
                            }
                            else
                            {
                                byte[] resFail = COMMON.Security.AES_Service.EncryptString("FAIL", authKey);
                                senderSocket.Send(new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.LoginResponse, resFail).Pack());
                                SERVER.LogUI.LogViewUI.AddLog($"[{senderIP}] đăng nhập thất bại (Sai mật khẩu)");
                            }
                        }
                    }
                }
                break;

            // 2. Tin nhắn bảo mật (Chat riêng)
            case PackageType.SecureMessage:
                if (clientKeys.TryGetValue(senderSocket, out byte[] senderKey))
                {
                    try
                    {
                        string decrypted = COMMON.Security.AES_Service.DecryptString(package.Content, senderKey);
                        if (decrypted.Contains("|"))
                        {
                            string[] parts = decrypted.Split('|');
                            string targetUser = parts[0]; // Bây giờ là Username đích
                            string messageContent = parts[1];

                            SERVER.LogUI.LogViewUI.AddLog($"Chat: [{senderIdentity}] -> [{targetUser}]: {messageContent}");

                            // Lưu DB (cần sửa logic DB nếu muốn lưu Username thay vì IP, tạm thời giữ nguyên tham số)
                            db.SaveChatMessage(senderIdentity, targetUser, messageContent);

                            ForwardToClient(targetUser, senderIdentity, messageContent, COMMON.DTO.PackageType.SecureMessage);
                        }
                    }
                    catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog($"!!! Lỗi SecureMessage: {ex.Message}"); }
                }
                break;

            // 3. Tin nhắn thường
            case PackageType.ChatMessage:
                // Biến msg ở đây là nguyên nhân gây lỗi cho các case khác nếu không đổi tên
                string msg = Encoding.UTF8.GetString(package.Content);
                SERVER.LogUI.LogViewUI.AddLog($"Tin nhắn thường từ [{senderIdentity}]: {msg}");
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

                if (!members.Contains(senderIdentity)) members.Add(senderIdentity);

                lock (groupMembersTable)
                {
                    groupMembersTable[newGroupName] = members;
                }

                // Thông báo cho tất cả thành viên (dựa trên Username)
                COMMON.DTO.DataPackage gUpdate = new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.GroupUpdate, Encoding.UTF8.GetBytes(newGroupName));
                BroadcastToList(members, gUpdate.Pack());
                break;

            // 5. Tin nhắn nhóm
            case PackageType.GroupMessage:
                if (clientKeys.TryGetValue(senderSocket, out byte[] sKey))
                {
                    try
                    {
                        string dec = COMMON.Security.AES_Service.DecryptString(package.Content, sKey);
                        string[] p = dec.Split('|');
                        if (p.Length < 2) return;

                        string gName = p[0];
                        string gContent = p[1];

                        SERVER.LogUI.LogViewUI.AddLog($"Group [{gName}]: {senderIdentity} -> {gContent}");
                        db.SaveGroupMessage(senderIdentity, gName, gContent);

                        if (groupMembersTable.ContainsKey(gName))
                        {
                            List<string> gMembers = groupMembersTable[gName];
                            // Gửi kèm senderIdentity (Username)
                            string forwardData = $"{gName}|{senderIdentity}|{gContent}";
                            BroadcastToGroup(gMembers, forwardData, senderSocket);
                        }
                    }
                    catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog($"!!! Lỗi GroupMsg: {ex.Message}"); }
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
                                    : db.GetChatHistory(senderIdentity, target);

                foreach (string line in logs)
                {
                    if (clientKeys.TryGetValue(senderSocket, out byte[] k))
                    {
                        byte[] enc = COMMON.Security.AES_Service.EncryptString(line, k);
                        COMMON.DTO.PackageType type = groupMembersTable.ContainsKey(target) ? COMMON.DTO.PackageType.GroupMessage : COMMON.DTO.PackageType.SecureMessage;
                        senderSocket.Send(new COMMON.DTO.DataPackage(type, enc).Pack());
                    }
                }
                break;

            // 8. Tín hiệu Video Call
            case PackageType.VideoCall:
                {
                    string vData = Encoding.UTF8.GetString(package.Content);
                    // Payload: [TargetUser]|[Action]|[Data]
                    if (!vData.Contains("|")) break;

                    string[] vParts = vData.Split('|');
                    string vTarget = vParts[0];
                    string vAction = vParts[1];
                    string vRawPayload = vParts.Length >= 3 ? vParts[2] : "";

                    // Giải mã dữ liệu video từ khóa người gửi và mã hóa lại bằng khóa người nhận
                    if (!string.IsNullOrEmpty(vRawPayload) && (vAction == "Frame" || vAction == "Audio"))
                    {
                        // Đổi tên biến thành vSenderKey để tránh lỗi trùng scope với SecureMessage
                        if (clientKeys.TryGetValue(senderSocket, out byte[] vSenderKey))
                        {
                            try
                            {
                                byte[] rawEncrypted = Convert.FromBase64String(vRawPayload);
                                byte[] decryptedVideo = COMMON.Security.AES_Service.Decrypt(rawEncrypted, vSenderKey);

                                // Tìm khóa của người nhận
                                byte[] targetKey = null;
                                lock (_socketToUser)
                                {
                                    foreach (var pair in _socketToUser)
                                    {
                                        if (pair.Value == vTarget && pair.Key.Connected)
                                        {
                                            if (clientKeys.TryGetValue(pair.Key, out targetKey))
                                                break;
                                        }
                                    }
                                }

                                if (targetKey != null)
                                {
                                    byte[] reEncryptedVideo = COMMON.Security.AES_Service.Encrypt(decryptedVideo, targetKey);
                                    vRawPayload = Convert.ToBase64String(reEncryptedVideo);
                                }
                            }
                            catch (Exception ex)
                            {
                                SERVER.LogUI.LogViewUI.AddLog($"Lỗi định tuyến VideoCall: {ex.Message}");
                            }
                        }
                    }

                    // ForwardToClient sẽ đóng gói và mã hóa lớp vỏ bảo vệ bên ngoài
                    ForwardToClient(vTarget, senderIdentity, $"{vAction}|{vRawPayload}", COMMON.DTO.PackageType.VideoCall);
                }
                break;
        }
    }

    // Gửi trực tiếp đến một client dựa trên IP

    private void ForwardToClient(string targetUsername, string senderUsername, string content, PackageType type)
    {
        lock (_socketToUser)
        {
            // Tìm socket có Username trùng với targetUsername
            foreach (var pair in _socketToUser)
            {
                if (pair.Value == targetUsername && pair.Key.Connected)
                {
                    // Tìm thấy socket đích, lấy Key mã hóa của họ
                    if (clientKeys.TryGetValue(pair.Key, out byte[] targetKey))
                    {
                        // Đóng gói: SenderUsername | Content
                        string dataToSend = (type == PackageType.SecureMessage || type == PackageType.VideoCall)
                                            ? $"{senderUsername}|{content}"
                                            : content;

                        byte[] enc = AES_Service.EncryptString(dataToSend, targetKey);
                        pair.Key.Send(new DataPackage(type, enc).Pack());
                    }
                    return; // Đã gửi xong thì thoát
                }
            }
        }
    }


    // Gửi cho danh sách IP cụ thể
    private void BroadcastToList(List<string> usernames, byte[] data)
    {
        lock (_socketToUser)
        {
            foreach (var pair in _socketToUser)
            {
                // Kiểm tra nếu Username nằm trong danh sách nhận
                if (usernames.Contains(pair.Value) && pair.Key.Connected)
                {
                    pair.Key.Send(data);
                }
            }
        }
    }

    // Gửi tin nhắn nhóm (có mã hóa riêng cho từng người)
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
        // Lấy IP trước khi đóng socket (dùng try-catch để an toàn)
        string clientIP = "Unknown";
        try { clientIP = client.RemoteEndPoint?.ToString(); } catch { }

        // Mặc định chuỗi cần xóa là IP (nếu chưa đăng nhập)
        string identityToRemove = clientIP;

        lock (_socketToUser)
        {
            if (_socketToUser.ContainsKey(client))
            {
                string uName = _socketToUser[client];

                // Nếu đã đăng nhập, chuỗi trên UI là "IP - Username", ta phải xóa đúng chuỗi này
                identityToRemove = $"{clientIP} - {uName}";

                _socketToUser.Remove(client);
            }
        }

        lock (clientKeys)
        {
            if (clientKeys.ContainsKey(client)) clientKeys.Remove(client);
        }

        try { client.Close(); } catch { }

        // Xóa khỏi giao diện Server
        if (!string.IsNullOrEmpty(identityToRemove))
        {
            SERVER.LogUI.LogViewUI.RemoveClient(identityToRemove);
            SERVER.LogUI.LogViewUI.AddLog($"Client [{identityToRemove}] đã ngắt kết nối.");
        }

        // Cập nhật danh sách mới cho các client còn lại
        BroadcastOnlineList();
    }

    //
    // Phát danh sách online cho tất cả client
    //
    private void BroadcastOnlineList()
    {
        lock (_socketToUser)
        {
            // 1. Lấy danh sách tất cả các Username đang kết nối
            List<string> allOnlineUsers = _socketToUser.Values.ToList();

            // 2. Gửi danh sách cho từng Client
            foreach (var pair in _socketToUser)
            {
                Socket client = pair.Key;
                string myName = pair.Value;

                if (client != null && client.Connected)
                {
                    try
                    {
                        // Lọc bỏ tên của chính mình
                        var filteredList = allOnlineUsers.Where(u => u != myName).ToList();

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

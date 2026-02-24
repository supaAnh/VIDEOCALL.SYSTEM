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
                // Tránh lỗi nếu server đã bị gán null
                if (server == null) break;

                Socket client = server.Accept();

                // 1. Gửi Key AES trước
                SendEncryptionKey(client);

                // 2. Bắt đầu luồng nhận dữ liệu
                Thread receive = new Thread(ReceiveData);
                receive.IsBackground = true;
                receive.Start(client);

                // 3. Cập nhật UI trên Server
                string clientIP = "Unknown";
                try { clientIP = client.RemoteEndPoint?.ToString(); } catch { }

                LogViewUI.AddClient(clientIP);
                LogViewUI.AddLog($" [{clientIP}]: đã kết nối");

                // Đợi 1 chút để Client khởi tạo Form Main xong
                Thread.Sleep(300);
                BroadcastOnlineList();
            }
        }
        catch (SocketException ex)
        {
            if (ex.SocketErrorCode != SocketError.Interrupted)
            {
                LogViewUI.AddLog("Lỗi Socket khi Accept: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            // Chỉ ghi log nếu server vẫn đang chạy mà gặp sự cố bất ngờ
            if (server != null)
            {
                LogViewUI.AddLog("Lỗi trong quá trình Accept: " + ex.Message);
            }
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

        // 1. Lấy IP ra trước ngay khi Socket còn đang kết nối để an toàn
        string clientIP = "Unknown";
        try { clientIP = senderSocket.RemoteEndPoint?.ToString(); } catch { }

        byte[] leftOver = new byte[0];

        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024 * 5000]; // 5MB
                int received = senderSocket.Receive(buffer);
                if (received == 0) break;

                byte[] dataToProcess = new byte[leftOver.Length + received];
                Buffer.BlockCopy(leftOver, 0, dataToProcess, 0, leftOver.Length);
                Buffer.BlockCopy(buffer, 0, dataToProcess, leftOver.Length, received);

                int offset = 0;
                while (offset + 5 <= dataToProcess.Length)
                {
                    int payloadLength = BitConverter.ToInt32(dataToProcess, offset + 1);
                    int totalPacketSize = 5 + payloadLength;

                    if (dataToProcess.Length >= offset + totalPacketSize)
                    {
                        byte[] packetBytes = new byte[totalPacketSize];
                        Buffer.BlockCopy(dataToProcess, offset, packetBytes, 0, totalPacketSize);

                        DataPackage package = DataPackage.Unpack(packetBytes);
                        HandlePackage(senderSocket, package);

                        offset += totalPacketSize;
                    }
                    else
                    {
                        break;
                    }
                }

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
            // Bỏ dòng log báo lỗi đỏ ở đây để tránh rác màn hình khi Server kích Client
        }
        finally
        {
            // 2. Truyền biến clientIP đã lấy được vào HandleDisconnect
            HandleDisconnect(senderSocket, clientIP);
        }
    }

    //
    //Hàm nhận data từ client
    //
    // Hàm xử lý logic tập trung cho từng gói tin sau khi đã được tách đúng

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

            // --- XỬ LÝ ĐĂNG KÝ ---
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

                // Gửi Tên Nhóm + Danh sách thành viên
                string updatePayload = $"{newGroupName}|{string.Join(",", members)}";
                COMMON.DTO.DataPackage gUpdate = new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.GroupUpdate, Encoding.UTF8.GetBytes(updatePayload));
                BroadcastToList(members, gUpdate.Pack());
                break;

            // Thêm xoá thành viên nhóm

            case PackageType.AddGroupMember:
            case PackageType.RemoveGroupMember:
                string actionData = Encoding.UTF8.GetString(package.Content);
                string[] actionParts = actionData.Split('|');
                if (actionParts.Length >= 2)
                {
                    string targetGroup = actionParts[0];
                    List<string> selectedUsers = actionParts[1].Split(',').ToList();

                    lock (groupMembersTable)
                    {
                        if (groupMembersTable.ContainsKey(targetGroup))
                        {
                            var currentGroupMembers = groupMembersTable[targetGroup];

                            // Chuỗi lưu trữ thông báo hệ thống
                            string systemMsg = "";

                            if (package.Type == PackageType.AddGroupMember)
                            {
                                // Thêm những người chưa có trong nhóm
                                foreach (var u in selectedUsers)
                                {
                                    if (!currentGroupMembers.Contains(u)) currentGroupMembers.Add(u);
                                }
                                systemMsg = $"Đã thêm {string.Join(", ", selectedUsers)} vào nhóm.";
                            }
                            else if (package.Type == PackageType.RemoveGroupMember)
                            {
                                // Xóa những người được chọn
                                foreach (var u in selectedUsers)
                                {
                                    if (currentGroupMembers.Contains(u)) currentGroupMembers.Remove(u);
                                }
                                systemMsg = $"Đã xoá {string.Join(", ", selectedUsers)} khỏi nhóm.";
                            }

                            // Cập nhật lại list trong bảng server
                            groupMembersTable[targetGroup] = currentGroupMembers;

                            // THÔNG BÁO CHO TOÀN BỘ THÀNH VIÊN HIỆN TẠI (VÀ CẢ NGƯỜI VỪA BỊ XÓA/VỪA ĐƯỢC THÊM)
                            // Để họ cập nhật lại danh sách hoặc xóa Group hiển thị
                            List<string> notifyList = new List<string>(currentGroupMembers);

                            // Nếu là xóa, phải gửi GroupUpdate kèm cho cả người bị xóa để họ biết (hoặc bạn có thể tạo gói tin riêng)
                            if (package.Type == PackageType.RemoveGroupMember)
                            {
                                foreach (var u in selectedUsers) if (!notifyList.Contains(u)) notifyList.Add(u);
                            }

                            string newPayload = $"{targetGroup}|{string.Join(",", currentGroupMembers)}";
                            COMMON.DTO.DataPackage updatePkg = new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.GroupUpdate, Encoding.UTF8.GetBytes(newPayload));
                            BroadcastToList(notifyList, updatePkg.Pack());

                            // GỬI TIN NHẮN HỆ THỐNG VÀO BOX CHAT CỦA NHÓM
                            string forwardData = $"{targetGroup}|Hệ thống|{systemMsg}";
                            db.SaveGroupMessage("Hệ thống", targetGroup, systemMsg);
                            // Truyền null vào tham số sender để tin nhắn gửi đến tất cả mọi người, kể cả người thao tác
                            BroadcastToGroup(notifyList, forwardData, null);
                        }
                    }
                }
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
                _fileHandler.ForwardFile(senderSocket, package, clientKeys, _socketToUser, groupMembersTable);
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
                    // Payload: [TargetUser/Group]|[Action]|[Data]
                    if (!vData.Contains("|")) break;

                    string[] vParts = vData.Split('|');
                    string vTarget = vParts[0]; // Có thể là Username hoặc Tên Nhóm
                    string vAction = vParts[1];
                    string vRawPayload = vParts.Length >= 3 ? vParts[2] : "";

                    // Kiểm tra xem vTarget có phải là một nhóm hay không
                    bool isGroup = false;
                    List<string> groupMembers = null;
                    lock (groupMembersTable)
                    {
                        if (groupMembersTable.ContainsKey(vTarget))
                        {
                            isGroup = true;
                            groupMembers = new List<string>(groupMembersTable[vTarget]);
                        }
                    }

                    if (isGroup)
                    {
                        // ----- XỬ LÝ GỌI NHÓM -----
                        if (clientKeys.TryGetValue(senderSocket, out byte[] vSenderKey))
                        {
                            byte[] decryptedVideo = null;
                            // 1. Giải mã Frame/Audio nếu có để chuẩn bị mã hóa lại cho từng thành viên
                            if (!string.IsNullOrEmpty(vRawPayload) && (vAction == "Frame" || vAction == "Audio"))
                            {
                                try
                                {
                                    byte[] rawEncrypted = Convert.FromBase64String(vRawPayload);
                                    decryptedVideo = COMMON.Security.AES_Service.Decrypt(rawEncrypted, vSenderKey);
                                }
                                catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog($"Lỗi giải mã Video Nhóm: {ex.Message}"); }
                            }

                            // 2. Broadcast cho tất cả thành viên (Trừ người gửi)
                            lock (_socketToUser)
                            {
                                foreach (var pair in _socketToUser)
                                {
                                    if (groupMembers.Contains(pair.Value) && pair.Key.Connected && pair.Key != senderSocket)
                                    {
                                        if (clientKeys.TryGetValue(pair.Key, out byte[] targetKey))
                                        {
                                            string payloadToSend = vRawPayload;

                                            if (decryptedVideo != null)
                                            {
                                                try
                                                {
                                                    byte[] reEncryptedVideo = COMMON.Security.AES_Service.Encrypt(decryptedVideo, targetKey);
                                                    payloadToSend = Convert.ToBase64String(reEncryptedVideo);
                                                }
                                                catch { continue; }
                                            }

                                            // Đóng gói 4 phần: TênNhóm | Action | NgườiGửiThựcSự | Payload
                                            string dataToSend = $"{vTarget}|{vAction}|{senderIdentity}|{payloadToSend}";
                                            byte[] enc = AES_Service.EncryptString(dataToSend, targetKey);
                                            pair.Key.Send(new DataPackage(COMMON.DTO.PackageType.VideoCall, enc).Pack());
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // ----- XỬ LÝ GỌI 1-1 (Logic nguyên bản) -----
                        if (!string.IsNullOrEmpty(vRawPayload) && (vAction == "Frame" || vAction == "Audio"))
                        {
                            if (clientKeys.TryGetValue(senderSocket, out byte[] vSenderKey))
                            {
                                try
                                {
                                    byte[] rawEncrypted = Convert.FromBase64String(vRawPayload);
                                    byte[] decryptedVideo = COMMON.Security.AES_Service.Decrypt(rawEncrypted, vSenderKey);

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

                        // ForwardToClient tự động kẹp senderIdentity vào, nên Client 1-1 sẽ nhận 3 phần
                        ForwardToClient(vTarget, senderIdentity, $"{vAction}|{vRawPayload}", COMMON.DTO.PackageType.VideoCall);
                    }
                }
                break;

            // 9. Lưu Record Video Call
            case PackageType.SaveRecord:
                try
                {
                    byte[] fullContent = package.Content;
                    int ipLength = BitConverter.ToInt32(fullContent, 0);
                    string targetIP = Encoding.UTF8.GetString(fullContent, 4, ipLength);

                    int headerSize = 4 + ipLength;
                    byte[] rawData = new byte[fullContent.Length - headerSize];
                    Buffer.BlockCopy(fullContent, headerSize, rawData, 0, rawData.Length);

                    // Giải nén JSON từ mảng byte
                    var fileDto = System.Text.Json.JsonSerializer.Deserialize<FilePackageDTO>(rawData);
                    if (fileDto != null)
                    {
                        // Lưu vào Database (senderIdentity là người bấm Record, targetIP là người đang gọi)
                        db.SaveVideoRecord(senderIdentity, targetIP, fileDto.FileName, fileDto.FileData);
                    }
                }
                catch (Exception ex)
                {
                    SERVER.LogUI.LogViewUI.AddLog("Lỗi khi nhận Record: " + ex.Message);
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
                string clientIdentity = "";
                // Lấy định danh thực sự của Client (Username nếu đã đăng nhập, ngược lại là IP)
                if (_socketToUser.ContainsKey(item.Key))
                {
                    clientIdentity = _socketToUser[item.Key];
                }
                else
                {
                    try { clientIdentity = item.Key.RemoteEndPoint.ToString(); } catch { }
                }

                // Kiểm tra xem định danh có nằm trong nhóm không
                if (members.Contains(clientIdentity) && item.Key.Connected && item.Key != sender)
                {
                    byte[] enc = AES_Service.EncryptString(rawData, item.Value);
                    item.Key.Send(new DataPackage(PackageType.GroupMessage, enc).Pack());
                }
            }
        }
    }

    // hàm xử lý ngắt kết nối 
    private void HandleDisconnect(Socket client, string knownIP)
    {
        string clientIP = knownIP;
        if (clientIP == "Unknown")
        {
            try { clientIP = client.RemoteEndPoint?.ToString(); } catch { }
        }

        string identityToRemove = clientIP;

        // Dùng cờ này để phân biệt: Client tự thoát HAY đã bị Server kích
        bool isAlreadyKicked = true;

        lock (_socketToUser)
        {
            if (_socketToUser.ContainsKey(client))
            {
                string uName = _socketToUser[client];
                identityToRemove = $"{clientIP} - {uName}";
                _socketToUser.Remove(client);
                isAlreadyKicked = false; // Vẫn tìm thấy trong danh sách -> Tự thoát
            }
        }

        lock (clientKeys)
        {
            if (clientKeys.ContainsKey(client))
            {
                clientKeys.Remove(client);
                isAlreadyKicked = false; // Vẫn tìm thấy trong danh sách -> Tự thoát
            }
        }

        try { client.Close(); } catch { }

        // CHỈ in log và gỡ khỏi giao diện UI nếu Client tự thoát (Chưa bị Kích)
        if (!isAlreadyKicked && !string.IsNullOrEmpty(identityToRemove) && identityToRemove != "Unknown")
        {
            SERVER.LogUI.LogViewUI.RemoveClient(identityToRemove);
            SERVER.LogUI.LogViewUI.AddLog($"Client [{identityToRemove}] đã ngắt kết nối.");
        }

        // Cập nhật danh sách Online mới nhất cho các Client khác
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


    //
    //  --- NGẮT KẾT NỐI ---
    //



    // Dừng Server và ngắt tất cả kết nối
    public void StopServer()
    {
        try
        {
            // Chuẩn bị thông báo ngắt kết nối cho tất cả client
            string message = "Server đã ngắt kết nối. Chương trình sẽ tự động đóng.";
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(message);

            // 1. Đóng Socket của tất cả Client đang online
            lock (_socketToUser)
            {
                // Dùng ToList() để tạo bản sao danh sách, tránh lỗi khi đang lặp mà xóa phần tử
                foreach (var client in _socketToUser.Keys.ToList())
                {
                    try
                    {
                        // Gửi thông báo cho Client biết Server đang tắt
                        client.Send(new DataPackage(PackageType.Notification, payload).Pack());

                        // Đợi một chút để gói tin kịp truyền đi qua mạng trước khi Socket bị đóng
                        System.Threading.Thread.Sleep(200);

                        client.Close();
                    }
                    catch { }
                }
                _socketToUser.Clear();
            }

            lock (clientKeys)
            {
                clientKeys.Clear();
            }

            lock (groupMembersTable)
            {
                groupMembersTable.Clear();
            }

            // 2. Đóng Socket Lắng nghe chính của Server
            if (server != null)
            {
                try { server.Close(); } catch { }
                server = null;
            }
        }
        catch (Exception ex)
        {
            SERVER.LogUI.LogViewUI.AddLog("Lỗi khi đóng Server: " + ex.Message);
        }
    }


    // Ngắt kết nối một Client cụ thể dựa trên định danh hiển thị
    public void DisconnectClient(string displayIdentity)
    {
        try
        {
            Socket clientToDisconnect = null;
            string username = "";

            // 1. Tìm Socket của Client sao cho khớp với định dạng "IP - Username" hoặc "IP"
            lock (_socketToUser)
            {
                foreach (var pair in _socketToUser)
                {
                    string ip = "";
                    try { ip = pair.Key.RemoteEndPoint?.ToString(); } catch { }

                    // Khớp với trường hợp đã đăng nhập "IP - Username" hoặc chưa đăng nhập "IP"
                    if (displayIdentity == $"{ip} - {pair.Value}" || displayIdentity == ip)
                    {
                        clientToDisconnect = pair.Key;
                        username = pair.Value; // Lấy đúng username để thông báo
                        break;
                    }
                }
            }

            if (clientToDisconnect != null)
            {
                // 2. Gửi thông báo cho Client biết họ đã bị Server ngắt kết nối
                string message = $"SERVER đã ngắt kết nối của {username}";
                byte[] payload = System.Text.Encoding.UTF8.GetBytes(message);

                try
                {
                    // Gửi thông báo
                    clientToDisconnect.Send(new DataPackage(PackageType.Notification, payload).Pack());

                    // Đợi một chút để gói tin kịp truyền đi qua mạng trước khi Socket bị đóng sập
                    System.Threading.Thread.Sleep(200);
                }
                catch { /* Bỏ qua lỗi nếu client đã rớt mạng từ trước */ }

                // 3. Đóng Socket và dọn dẹp khỏi các danh sách quản lý
                try { clientToDisconnect.Close(); } catch { }

                lock (_socketToUser)
                {
                    _socketToUser.Remove(clientToDisconnect);
                }

                lock (clientKeys)
                {
                    if (clientKeys.ContainsKey(clientToDisconnect))
                    {
                        clientKeys.Remove(clientToDisconnect);
                    }
                }

                SERVER.LogUI.LogViewUI.AddLog($"Đã ngắt kết nối Client: {username}.");
                BroadcastOnlineList();
            }
            else
            {
                SERVER.LogUI.LogViewUI.AddLog($"Không tìm thấy dữ liệu Socket cho: {displayIdentity}");
            }
        }
        catch (Exception ex)
        {
            SERVER.LogUI.LogViewUI.AddLog("Lỗi khi kích Client: " + ex.Message);
        }
    }



}

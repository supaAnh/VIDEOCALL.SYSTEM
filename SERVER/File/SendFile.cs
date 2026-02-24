using COMMON.DTO;
using COMMON.Security;
using SERVER.Database;
using SERVER.LogUI;
using System.Net.Sockets;
using System.Text;

namespace SERVER.File
{
    public class SendFile
    {


        public void ForwardFile(Socket senderSocket, COMMON.DTO.DataPackage package, Dictionary<Socket, byte[]> clientKeys, Dictionary<Socket, string> socketToUser, Dictionary<string, List<string>> groupMembersTable)
        {
            try
            {
                byte[] fullContent = package.Content;
                if (fullContent.Length < 4) return;

                // 1. Đọc độ dài định danh mục tiêu (Username, IP hoặc Tên Nhóm)
                int identityLength = BitConverter.ToInt32(fullContent, 0);

                // 2. Trích xuất định danh mục tiêu
                string targetIdentity = Encoding.UTF8.GetString(fullContent, 4, identityLength);

                // 3. Trích xuất phần dữ liệu file đã mã hóa
                int headerSize = 4 + identityLength;
                byte[] fileEncrypted = new byte[fullContent.Length - headerSize];
                Buffer.BlockCopy(fullContent, headerSize, fileEncrypted, 0, fileEncrypted.Length);

                // 4. GIẢI MÃ dữ liệu thô bằng khóa của người gửi (chỉ cần làm 1 lần)
                byte[] decryptedRaw = null;
                lock (clientKeys)
                {
                    if (clientKeys.ContainsKey(senderSocket))
                    {
                        decryptedRaw = COMMON.Security.AES_Service.Decrypt(fileEncrypted, clientKeys[senderSocket]);
                    }
                }

                if (decryptedRaw == null) return;

                // 5. Kiểm tra xem định danh mục tiêu có phải là một Nhóm không
                bool isGroup = false;
                List<string> groupMembers = null;
                lock (groupMembersTable)
                {
                    if (groupMembersTable.ContainsKey(targetIdentity))
                    {
                        isGroup = true;
                        groupMembers = new List<string>(groupMembersTable[targetIdentity]);
                    }
                }

                if (isGroup)
                {
                    // --- XỬ LÝ GỬI FILE VÀO NHÓM ---
                    string senderIdentity = "";
                    lock (socketToUser)
                    {
                        if (socketToUser.ContainsKey(senderSocket))
                            senderIdentity = socketToUser[senderSocket];
                        else
                            senderIdentity = senderSocket.RemoteEndPoint.ToString();
                    }

                    lock (socketToUser)
                    {
                        foreach (var pair in socketToUser)
                        {
                            // Nếu thành viên thuộc nhóm, đang online và KHÔNG PHẢI người gửi
                            if (groupMembers.Contains(pair.Value) && pair.Key.Connected && pair.Key != senderSocket)
                            {
                                Socket memberSocket = pair.Key;
                                lock (clientKeys)
                                {
                                    if (clientKeys.ContainsKey(memberSocket))
                                    {
                                        // MÃ HÓA LẠI bằng khóa của từng người nhận
                                        byte[] reEncrypted = COMMON.Security.AES_Service.Encrypt(decryptedRaw, clientKeys[memberSocket]);
                                        memberSocket.Send(new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.SendFile, reEncrypted).Pack());
                                    }
                                }
                            }
                        }
                    }
                    SERVER.LogUI.LogViewUI.AddLog($"Chuyển tiếp file vào nhóm [{targetIdentity}] từ [{senderIdentity}]");
                }
                else
                {
                    // --- XỬ LÝ GỬI FILE 1-1 ---
                    Socket targetSocket = null;

                    // Tìm Socket đích dựa trên Username (nếu người dùng đã đăng nhập)
                    lock (socketToUser)
                    {
                        foreach (var pair in socketToUser)
                        {
                            if (pair.Value == targetIdentity && pair.Key.Connected)
                            {
                                targetSocket = pair.Key;
                                break;
                            }
                        }
                    }

                    // Nếu không tìm thấy bằng Username, dự phòng tìm bằng IP
                    if (targetSocket == null)
                    {
                        lock (clientKeys)
                        {
                            foreach (var item in clientKeys)
                            {
                                if (item.Key.RemoteEndPoint.ToString() == targetIdentity && item.Key.Connected)
                                {
                                    targetSocket = item.Key;
                                    break;
                                }
                            }
                        }
                    }

                    if (targetSocket != null)
                    {
                        lock (clientKeys)
                        {
                            if (clientKeys.ContainsKey(targetSocket))
                            {
                                // MÃ HÓA LẠI bằng khóa của người nhận
                                byte[] reEncrypted = COMMON.Security.AES_Service.Encrypt(decryptedRaw, clientKeys[targetSocket]);
                                targetSocket.Send(new COMMON.DTO.DataPackage(COMMON.DTO.PackageType.SendFile, reEncrypted).Pack());
                                SERVER.LogUI.LogViewUI.AddLog($"Chuyển tiếp file: {senderSocket.RemoteEndPoint} -> {targetIdentity}");
                            }
                        }
                    }
                    else
                    {
                        SERVER.LogUI.LogViewUI.AddLog($"Không tìm thấy người nhận: {targetIdentity} để chuyển file.");
                    }
                }
            }
            catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog("Lỗi chuyển tiếp file: " + ex.Message); }
        }

    }
}
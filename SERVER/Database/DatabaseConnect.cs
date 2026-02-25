using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using SERVER.LogUI;

namespace SERVER.Database
{
    public class DatabaseConnect
    {
        // Chuỗi kết nối tới Docker Container dbVideocall trên port 1555
        private readonly string _connectionString = "Server=127.0.0.1,1555;Database=dbVideocall;User Id=sa;Password=@Supanh123;TrustServerCertificate=True;";

        //Check kết nối đến SQL Server
        public bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open(); // Thử mở kết nối
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra UI của Server
                SERVER.LogUI.LogViewUI.AddLog(">>> LỖI KẾT NỐI DATABASE: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Lưu tin nhắn đã mã hóa vào SQL Server
        /// </summary>
        public void SaveChatMessage(string senderIP, string receiverIP, string encryptedContent)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Đổi 'Content' thành 'ContentVarbinary' để khớp với ảnh SQL của bạn
                    string query = "INSERT INTO ChatHistory (SenderIP, ReceiverIP, ContentVarbinary, Timestamp) " +
                                   "VALUES (@sender, @receiver, @content, @time)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@sender", senderIP);
                    cmd.Parameters.AddWithValue("@receiver", receiverIP);

                    // Nếu cột là Varbinary, nên chuyển string thành mảng byte
                    cmd.Parameters.AddWithValue("@content", Encoding.UTF8.GetBytes(encryptedContent));
                    cmd.Parameters.AddWithValue("@time", DateTime.Now);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    LogViewUI.AddLog(" >>> Lưu tin nhắn thành công");
                }
            }
            catch (Exception ex)
            {
                // Kiểm tra lỗi tại đây trên giao diện Server
                SERVER.LogUI.LogViewUI.AddLog("Lỗi SQL (Save): " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy lịch sử tin nhắn giữa 2 người dùng
        /// </summary>
        public List<string> GetChatHistory(string userA, string userB)
        {
            List<string> history = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Lấy tin nhắn qua lại giữa A và B, sắp xếp theo thời gian
                    string query = "SELECT SenderIP, ContentVarbinary, Timestamp FROM ChatHistory " +
                                   "WHERE (SenderIP = @userA AND ReceiverIP = @userB) " +
                                   "OR (SenderIP = @userB AND ReceiverIP = @userA) " +
                                   "ORDER BY Timestamp ASC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userA", userA);
                    cmd.Parameters.AddWithValue("@userB", userB);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string sender = reader["SenderIP"].ToString();
                            byte[] contentRaw = (byte[])reader["ContentVarbinary"];
                            string content = Encoding.UTF8.GetString(contentRaw);
                            history.Add($"{sender}|{content}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SERVER.LogUI.LogViewUI.AddLog("Lỗi SQL (GetHistory): " + ex.Message);
            }
            return history;
        }



        /// <summary>
        /// CHAT GROUP - Lưu tin nhắn nhóm đã mã hóa vào SQL Server
        /// </summary>
        public void SaveGroupMessage(string senderIP, string groupName, string message)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Trỏ vào bảng GroupChatHistory
                    string query = "INSERT INTO GroupChatHistory (SenderIP, GroupName, ContentVarbinary, Timestamp) " +
                                   "VALUES (@sender, @group, @content, @time)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@sender", senderIP);
                    cmd.Parameters.AddWithValue("@group", groupName);
                    cmd.Parameters.AddWithValue("@content", Encoding.UTF8.GetBytes(message));
                    cmd.Parameters.AddWithValue("@time", DateTime.Now);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogViewUI.AddLog("Lỗi SQL Group (Save): " + ex.Message);
            }
        }

        public List<string> GetGroupChatHistory(string groupName)
        {
            List<string> history = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Truy vấn từ bảng GroupChatHistory
                    string query = "SELECT SenderIP, ContentVarbinary FROM GroupChatHistory " +
                                   "WHERE GroupName = @group ORDER BY Timestamp ASC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@group", groupName);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string sender = reader["SenderIP"].ToString();
                            byte[] contentRaw = (byte[])reader["ContentVarbinary"];
                            string content = Encoding.UTF8.GetString(contentRaw);
                            // Format: TenNhom|NguoiGui|NoiDung
                            history.Add($"{groupName}|{sender}|{content}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogViewUI.AddLog("Lỗi SQL Group (Get): " + ex.Message);
            }
            return history;
        }



        /// <summary>
        ///  FILE TRANSFER - Lưu thông tin file đã gửi vào SQL Server
        /// </summary> 
        /// 
        
        public void SaveFileHistory(string senderIP, string receiverIP, string fileName, byte[] fileData)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO ChatHistory (SenderIP, ReceiverIP, ContentVarbinary, Timestamp) VALUES (@sender, @receiver, @content, @time)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@sender", senderIP);
                    cmd.Parameters.AddWithValue("@receiver", receiverIP);
                    // Lưu tên file + data hoặc chỉ data tùy thiết kế
                    cmd.Parameters.AddWithValue("@content", fileData);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { LogViewUI.AddLog("Lỗi lưu file SQL: " + ex.Message); }
        }




        //
        //      --- LOGIN & REGISTER DATABASE METHODS ---   
        //


        // Đăng ký user mới
        public bool RegisterUser(string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Kiểm tra user tồn tại chưa
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @u";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@u", username);

                    conn.Open();
                    int exist = (int)checkCmd.ExecuteScalar();
                    if (exist > 0) return false; // Tài khoản đã tồn tại

                    // Thêm mới
                    string query = "INSERT INTO Users (Username, Password) VALUES (@u, @p)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                SERVER.LogUI.LogViewUI.AddLog("Lỗi Register DB: " + ex.Message);
                return false;
            }
        }

        // Kiểm tra đăng nhập
        public bool CheckLogin(string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM Users WHERE Username = @u AND Password = @p";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                SERVER.LogUI.LogViewUI.AddLog("Lỗi Login DB: " + ex.Message);
                return false;
            }
        }


        //
        //  --- RECORD VIDEOCALL---
        //

        // 1. CẬP NHẬT LẠI HÀM SaveVideoRecord để lưu FileName
        public void SaveVideoRecord(string senderIP, string receiverIP, string fileName, byte[] videoData)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Chuyển sang lưu vào bảng VideoRecords riêng biệt để dễ quản lý file
                    string query = "INSERT INTO VideoRecords (SenderIP, ReceiverIP, FileName, VideoData, Timestamp) VALUES (@sender, @receiver, @filename, @content, @time)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@sender", senderIP);
                    cmd.Parameters.AddWithValue("@receiver", receiverIP);
                    cmd.Parameters.AddWithValue("@filename", fileName); // Đã thêm lưu tên file
                    cmd.Parameters.AddWithValue("@content", videoData);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    SERVER.LogUI.LogViewUI.AddLog($"Đã lưu bản Record ({fileName}) của {senderIP} vào DB.");
                }
            }
            catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog("Lỗi lưu DB Record: " + ex.Message); }
        }

        // Lấy danh sách những User đã từng gửi Record
        public List<string> GetUsersWithRecords()
        {
            List<string> users = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT DISTINCT SenderIP FROM VideoRecords";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(reader["SenderIP"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog("Lỗi lấy danh sách User Record: " + ex.Message); }
            return users;
        }

        // Lấy danh sách tên file Record của 1 User cụ thể
        public List<string> GetRecordsByUser(string username)
        {
            List<string> records = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT FileName FROM VideoRecords WHERE SenderIP = @user ORDER BY Timestamp DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", username);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(reader["FileName"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog("Lỗi lấy danh sách File Record: " + ex.Message); }
            return records;
        }

        // Tải mảng byte[] của Video dựa vào FileName
        public byte[] GetVideoData(string fileName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT TOP 1 VideoData FROM VideoRecords WHERE FileName = @filename";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@filename", fileName);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return (byte[])result;
                    }
                }
            }
            catch (Exception ex) { SERVER.LogUI.LogViewUI.AddLog("Lỗi tải Data Video: " + ex.Message); }
            return null;
        }



        //
        //   --- SERVER LOG ---
        //
        public void SaveServerLog(string sessionId, string message)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO ServerLogs (SessionID, LogTime, LogMessage) VALUES (@session, @time, @msg)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@session", sessionId);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now);
                    cmd.Parameters.AddWithValue("@msg", message);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                // Nếu lỗi kết nối DB khi đang ghi Log thì cứ bỏ qua để Server không bị sập
            }
        }


        /// <summary>
        /// Lấy danh sách các Session đã lưu trong Database (kèm thời gian bắt đầu)
        /// </summary>
        public Dictionary<string, string> GetSessionList()
        {
            Dictionary<string, string> sessions = new Dictionary<string, string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Lấy SessionID và thời gian log đầu tiên của session đó để hiển thị cho dễ nhìn
                    string query = "SELECT SessionID, MIN(LogTime) as StartTime FROM ServerLogs GROUP BY SessionID ORDER BY StartTime DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string sId = reader["SessionID"].ToString();
                            DateTime sTime = Convert.ToDateTime(reader["StartTime"]);
                            // Định dạng hiển thị: "dd/MM/yyyy HH:mm:ss - [SessionID]"
                            string displayStr = $"{sTime.ToString("dd/MM/yyyy HH:mm:ss")} - {sId}";
                            sessions.Add(sId, displayStr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Không in log ra UI nếu lỗi lấy danh sách
            }
            return sessions;
        }

        /// <summary>
        /// Lấy toàn bộ Log của 1 Session cụ thể
        /// </summary>
        public List<string[]> GetLogsBySession(string sessionId)
        {
            List<string[]> logs = new List<string[]>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT LogTime, LogMessage FROM ServerLogs WHERE SessionID = @session ORDER BY LogTime ASC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@session", sessionId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string time = Convert.ToDateTime(reader["LogTime"]).ToString("HH:mm:ss");
                            string msg = reader["LogMessage"].ToString();
                            logs.Add(new string[] { time, msg });
                        }
                    }
                }
            }
            catch (Exception) { }
            return logs;
        }


    }
}
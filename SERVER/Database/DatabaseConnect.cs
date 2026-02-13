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
    }
}
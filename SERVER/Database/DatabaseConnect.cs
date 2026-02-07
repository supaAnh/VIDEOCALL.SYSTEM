using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient; // Đảm bảo đã cài NuGet package này
using System.Text;

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
                    string query = "INSERT INTO ChatHistory (SenderIP, ReceiverIP, Content, Timestamp) " +
                                   "VALUES (@sender, @receiver, @content, @time)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@sender", senderIP);
                    cmd.Parameters.AddWithValue("@receiver", receiverIP);
                    cmd.Parameters.AddWithValue("@content", encryptedContent);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Gọi Log từ LogUI đã có của bạn để theo dõi lỗi
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
                    string query = "SELECT SenderIP, Content FROM ChatHistory " +
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
                            string content = reader["Content"].ToString();
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
    }
}
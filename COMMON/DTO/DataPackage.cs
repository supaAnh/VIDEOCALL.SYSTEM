using System;
using System.Collections.Generic;
using System.Text;

namespace COMMON.DTO
{
    public enum PackageType
    {
        Authenticate = 0, // Gửi username và password để xác thực
        ChatMessage = 1, // Gửi tin nhắn văn bản
        SendFile = 2, // Gửi file
        VideoCall = 3, // Gọi video
        VoiceCall = 4, // Gọi thoại
        UserStatusUpdate = 5, // Cập nhật trạng thái người dùng (online/offline)
        DH_KeyExchange = 6,   // Gói tin trao đổi khóa công khai DH
        SecureMessage = 7, // Gửi tin nhắn đã mã hóa
        RequestChatHistory = 8, // Yêu cầu lịch sử chat
        CreateGroup = 9,      // Client yêu cầu tạo nhóm
        GroupUpdate = 10,     // Server thông báo có nhóm mới cho các thành viên
        GroupMessage = 11,     // Tin nhắn gửi trong nhóm
        VideoCallSignal = 12, // Tín hiệu điều khiển cuộc gọi video (như yêu cầu, từ chối, kết thúc)
        Register = 13,      // Loại gói tin đăng ký
        LoginResponse = 14,  // Server trả về kết quả đăng nhập
        SaveRecord = 15,     // Gói tin lưu trữ bản ghi cuộc gọi
        Notification = 16,     // Gói tin thông báo chung (có thể dùng cho nhiều mục đích khác nhau)
    }

    public class DataPackage
    {
        public PackageType Type { get; set; }
        public byte[] Content { get; set; }

        public DataPackage(PackageType type, byte[] content)
        {
            this.Type = type;
            this.Content = content;
        }

        public byte[] Pack()
        {
            byte[] packet = new byte[1 + 4 + Content.Length];
            packet[0] = (byte)Type;

            // Ghi độ dài nội dung (4 byte)
            byte[] lengthBytes = BitConverter.GetBytes(Content.Length);
            Array.Copy(lengthBytes, 0, packet, 1, 4);

            //Chép nội dung
            Array.Copy(Content, 0, packet, 5, Content.Length);

            return packet;
        }

        public static DataPackage Unpack(byte[] data)
        {
            PackageType type = (PackageType)data[0];
            int length = BitConverter.ToInt32(data, 1);
            byte[] content = new byte[length];
            Array.Copy(data, 5, content, 0, length);

            return new DataPackage(type, content);
        }
    }
}

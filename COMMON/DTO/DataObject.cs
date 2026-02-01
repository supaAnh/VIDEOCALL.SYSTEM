[Serializable]
public class DataObject
{
    public string SenderName { get; set; }
    public string ReceiverName { get; set; } // Dùng để nhắn tin riêng
    public MessageType Type { get; set; }    // Chat, File, VideoCall
    public byte[] Data { get; set; }         // Nội dung đã mã hóa AES
}

public enum MessageType { Message, Photo, File, VideoCall, Authenticate }
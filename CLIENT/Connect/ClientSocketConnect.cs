using COMMON.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

public class ClientSocketConnect
{
    private Socket client;
    private byte[] buffer = new byte[1024 * 5000]; // Buffer nhận dữ liệu thô từ mạng

    // Buffer dùng để gom các mảnh gói tin bị phân mảnh
    private List<byte> _packetBuffer = new List<byte>();

    public byte[] AesKey { get; private set; }

    // Sự kiện trả về Gói tin hoàn chỉnh (Full Packet)
    public event Action<byte[]> OnRawDataReceived;
    public event Action OnDisconnected;

    public void Connect(int port)
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            client.Connect(new IPEndPoint(IPAddress.Loopback, port));
            StartReceiving();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể kết nối tới server: " + ex.Message);
        }
    }

    private void StartReceiving()
    {
        try
        {
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }
        catch { HandleDisconnect(); }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            int received = client.EndReceive(ar);
            if (received > 0)
            {
                // 1. Đưa dữ liệu mới nhận vào Packet Buffer
                byte[] chunk = new byte[received];
                Array.Copy(buffer, 0, chunk, 0, received);
                lock (_packetBuffer)
                {
                    _packetBuffer.AddRange(chunk);
                }

                // 2. Xử lý cắt gói tin (Packet Slicing)
                ProcessBuffer();

                // 3. Tiếp tục nhận dữ liệu
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            else { HandleDisconnect(); }
        }
        catch { HandleDisconnect(); }
    }

    // Hàm quan trọng: Cắt buffer thành các gói tin hoàn chỉnh
    private void ProcessBuffer()
    {
        lock (_packetBuffer)
        {
            while (_packetBuffer.Count > 5) // Header tối thiểu 5 byte (1 Type + 4 Length)
            {
                // Đọc độ dài gói tin (byte thứ 1 đến 4)
                byte[] lengthBytes = _packetBuffer.GetRange(1, 4).ToArray();
                int contentLen = BitConverter.ToInt32(lengthBytes, 0);
                int totalPacketLen = 1 + 4 + contentLen; // Tổng độ dài gói tin

                if (_packetBuffer.Count >= totalPacketLen)
                {
                    // Đã nhận ĐỦ gói tin -> Cắt ra xử lý
                    byte[] packetData = _packetBuffer.GetRange(0, totalPacketLen).ToArray();
                    _packetBuffer.RemoveRange(0, totalPacketLen); // Xóa khỏi buffer

                    // Xử lý gói tin hoàn chỉnh
                    HandlePacket(packetData);
                }
                else
                {
                    // Chưa đủ dữ liệu -> Thoát vòng lặp, đợi nhận tiếp
                    break;
                }
            }
        }
    }

    private void HandlePacket(byte[] data)
    {
        try
        {
            // Kiểm tra Key Exchange
            // Lưu ý: Unpack ở đây an toàn vì ta đã đảm bảo data là gói tin trọn vẹn
            var package = DataPackage.Unpack(data);
            if (package.Type == PackageType.DH_KeyExchange)
            {
                this.AesKey = package.Content;
            }

            // Bắn sự kiện gói tin hoàn chỉnh ra ngoài
            OnRawDataReceived?.Invoke(data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Packet Error: " + ex.Message);
        }
    }

    private void HandleDisconnect()
    {
        try { client?.Close(); } catch { }
        OnDisconnected?.Invoke();
    }

    public void Send(byte[] data)
    {
        if (client != null && client.Connected)
        {
            client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        try { ((Socket)ar.AsyncState).EndSend(ar); } catch { }
    }
}
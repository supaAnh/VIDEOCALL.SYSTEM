using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

public class ClientSocketConnect
{
    private Socket client;
    private byte[] buffer = new byte[1024 * 5000]; // 5MB

    // Sự kiện để báo về Form khi có tin nhắn mới
    public event Action<string> OnDataReceived;
    // Sự kiện báo mất kết nối
    public event Action OnDisconnected;

    // Kết nối tới server
    public void Connect(int port)
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            // Kết nối đồng bộ
            client.Connect(new IPEndPoint(IPAddress.Loopback, port));

            StartReceiving();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể kết nối tới server: " + ex.Message);
        }
    }

    // Bắt đầu nhận dữ liệu bất đồng bộ
    private void StartReceiving()
    {
        // Bắt đầu nhận dữ liệu bất đồng bộ
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }

    // Callback khi có dữ liệu nhận được
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            int received = client.EndReceive(ar);
            if (received > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, received);

                // Bắn sự kiện về Form
                OnDataReceived?.Invoke(message);

                // Tiếp tục đợi gói tin tiếp theo 
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            else
            {
                HandleDisconnect();
            }
        }
        catch
        {
            HandleDisconnect();
        }
    }

    // Xử lý mất kết nối
    private void HandleDisconnect()
    {
        client.Close();
        OnDisconnected?.Invoke(); // Báo mất kết nối
    }

    // Gửi dữ liệu đến server
    public void Send(byte[] data)
    {
        // Kiểm tra xem socket đã được khởi tạo và còn kết nối không
        if (client != null && client.Connected)
        {
            // Gọi BeginSend từ đối tượng Socket thực sự
            client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
        }
    }
    // Callback hoàn tất gửi dữ liệu
    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket s = (Socket)ar.AsyncState;
            s.EndSend(ar); // Hoàn tất việc gửi
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi SendCallback: " + ex.Message);
        }
    }

}
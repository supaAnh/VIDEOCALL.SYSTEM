using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;

public class SocketConnect
{
    //Địa chỉ IP và cổng của server
    IPEndPoint IP;
    Socket server;
    List<Socket> clientList;
    
    public void StartServer(int port)
    {
        clientList = new List<Socket>();
        //Khởi tạo địa chỉ IP và cổng
        IP = new IPEndPoint(IPAddress.Any, port);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        server.Bind(IP);

        //Lắng nghe kết nối từ client
        Thread listen = new Thread(() =>
        {
            try
            {
                while (true)
                {
                    server.Listen(100);
                    Socket client = server.Accept(); //Chấp nhận kết nối từ client
                    clientList.Add(client);
                    //Tạo luồng nhận dữ liệu từ client
                    Thread receive = new Thread(ReceiveData);
                    receive.IsBackground = true;
                    receive.Start(client);
                    MessageBox.Show($" [{client.RemoteEndPoint.ToString()}]: đã kết nối" , "Client kết nối");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        });
        listen.IsBackground = true;
        listen.Start();
    }


    //Hàm nhận data từ client
    public void ReceiveData(object obj)
    {
        Socket client = obj as Socket; //Ép kiểu đối tượng về Socket
        try
        {
            while (true)
            {
                byte[] data = new byte[1024 * 5000]; //Bộ đệm nhận dữ liệu buffer 5000kb
                client.Receive(data); //Nhận dữ liệu từ client
                //Xử lý dữ liệu nhận được
                
                foreach (Socket item in clientList)
                {
                    if (item != null && item != client)
                        item.Send(data); //Gửi dữ liệu đến tất cả client trừ client gửi
                }

            }
        }
        catch (Exception ex)
        {
            clientList.Remove(client);
            client.Close();
        }

    }
}

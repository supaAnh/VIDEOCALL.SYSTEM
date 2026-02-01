using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SERVER.Message
{
    public class MesssageSend
    {
        //Phát dữ liệu đến tất cả client trong danh sách, trừ client gửi
        public void Broadcast(byte[] data, Socket sender, List<Socket> clientList)
        {
            lock(clientList) //An toàn cho đa luồng
            {
                foreach(Socket client in clientList)
                {
                    //Không gửi lại cho chính client gửi
                    if (client != sender && client.Connected)
                    {
                        try
                        {
                            client.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
                            {
                                client.EndSend(ar);
                            }, null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi gửi dữ liệu: " + ex.Message);
                        }
                    }
                }
            }    
        }

        //Gửi dữ liệu đến một client cụ thể
        public void SendToTarget(byte[] data, Socket targetSocket)
        {
            if(targetSocket != null && targetSocket.Connected)
            {
                try
                {
                    targetSocket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
                    {
                        targetSocket.EndSend(ar);
                    }, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi gửi dữ liệu: " + ex.Message);
                }
            }
        }


    }
}
 

using System.Net.Sockets;

namespace SERVER.Process
{
    public static class MessageDispatcher
    {
        public static void ForwardToTarget(string targetIP, byte[] fullPackage, Dictionary<Socket, byte[]> clientKeys)
        {
            lock (clientKeys)
            {
                foreach (var client in clientKeys.Keys)
                {
                    // So sánh IP của client trong danh sách với IP mục tiêu
                    if (client.RemoteEndPoint.ToString() == targetIP && client.Connected)
                    {
                        try { client.Send(fullPackage); } catch { }
                        break; 
                    }
                }
            }
        }
    }
}
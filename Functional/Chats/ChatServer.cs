using System.Net;
using Common.Logging;
using System.Threading;
using System.Net.Sockets;

namespace Common.Chats.Server
{
    /// <summary>
    /// Class provides incomming point for start of chating.
    /// Class has function "Initiation()" with start server listening.
    /// </summary>
    public static class ChatServer
    {
        public static ChatModule module = new ChatModule();
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public static void Initiation(string IP, int Port)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
            socket.Bind(iPEndPoint);
            socket.Listen(100000);
            Thread thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Start();
            Logger.WriteLog("Initiation chat module. Server state->listening. IP->" + IP + " Port->" + Port + ".", LogLevel.Usual);
        }
        private static void Listen()
        {
            while (true)
            {
                Socket handleSocket = socket.Accept();
                Thread thread = new Thread(() => HandleChatUser(ref handleSocket))
                {
                    IsBackground = true
                };
                thread.Start();
            }
        }
        private static void HandleChatUser(ref Socket handleSocket)
        {
            int bytes = 0;
            byte[] buffer = new byte[1096];
            for (; ; )
            {
                if (bytes + handleSocket.Available < 1096)
                {
                    bytes += handleSocket.Receive(buffer, bytes, handleSocket.Available, SocketFlags.None);
                }
                else
                {
                    bytes += handleSocket.Receive(buffer, bytes, (1096 - bytes), SocketFlags.None);
                    break;
                }
                if (handleSocket.Available == 0 && bytes > 0)
                {
                    break;
                }
                if (handleSocket.Available == 0)
                {
                    if ((handleSocket.Poll(10000, SelectMode.SelectRead) && (handleSocket.Available == 0)) || !handleSocket.Connected)
                    {
                        Logger.WriteLog("Remote socket was disconnected. Address->" + handleSocket.AddressFamily.ToString(), LogLevel.Usual);
                        handleSocket.Close();
                        break;
                    }
                }
            }
            if (handleSocket.Connected)
            {
                module.IdentifyChatUser(ref handleSocket, ref buffer, ref bytes);
            }
        }
    }
}

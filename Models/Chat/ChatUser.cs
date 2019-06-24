using System.Net.Sockets;

namespace Common.Chats
{
    public class ChatUser
    {
        public int chat_id;
        public int user_id;
        public bool enable = false;
        public Socket remoteSocket;
    }
}

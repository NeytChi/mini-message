using System.Net.Sockets;

namespace Common.Chats
{
    public class ChatUser
    {
        public int chatuser_id;
        public int user_id;
        public ChatRoom chat_room;
        public string user_login;
        public bool enable = false;
        public Socket remoteSocket;
    }
}

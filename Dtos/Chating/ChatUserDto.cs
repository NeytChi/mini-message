using System.Net.Sockets;

namespace mini_message.Dtos.Chating
{
    public class ChatUserDto
    {
        public int chat_id;
        public int user_id;
        public bool enable = false;
        public Socket remoteSocket;
    }
}

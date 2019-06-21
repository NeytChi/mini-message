using System;
using System.Text;
using Common.Logging;
using Common.NDatabase;
using System.Net.Sockets;
using Common.NDatabase.UserData;
using System.Collections.Generic;

namespace Common.Chats
{
    public class ChatModule
    {
        private bool request_view = true;
        private List<ChatRoom> rooms = new List<ChatRoom>();

        public void IdentifyChatUser(ref Socket remoteSocket, ref byte[] buffer, ref int bytes)
        {
            string connection = Encoding.UTF8.GetString(buffer, 0, bytes);
            if (request_view)
            {
                Console.WriteLine(connection);
            }
            string user_token = GetProtocolParameter(ref connection, 1, TypeCode.String);
            UserCache user = new UserCache();
            if (Database.user.SelectUserByToken(user_token, ref user))
            {
                ChatUser chatUser = new ChatUser();
                chatUser.user_id = user.user_id;
                chatUser.chatuser_id = user.user_id;
                chatUser.user_id = user.user_id;
                chatUser.enable = true;
                chatUser.user_login = user.user_login;
                chatUser.remoteSocket = remoteSocket;
                Processing(ref chatUser);
            }
            else
            {
                string answer = "Can't find user_token by request's body.";
                Logger.WriteLog(answer, LogLevel.Warning);
                SendSocket(ref remoteSocket, ref answer);
            }
        }
        public void Processing(ref ChatUser user)
        {
            for (; ; )
            {
                string request = ReceiveMessage(ref user);
                if (!user.enable)
                {
                    EndConnect(ref user.chat_room, ref user);
                    break;
                }
                if (!string.IsNullOrEmpty(request))
                {
                    HandleMessage(ref request, ref user);
                }
            }
        }
        public void SendSocket(ref Socket remoteSocket,ref string message)
        {
            if (remoteSocket != null)
            {
                if (remoteSocket.Connected)
                {
                    byte[] buffer_send = Encoding.UTF8.GetBytes(message);
                    remoteSocket.Send(buffer_send);
                }
            }
        }
        public string ReceiveMessage(ref ChatUser user)
        {
            int bytes = 0;
            string request = "";
            byte[] buffer = new byte[2096];
            do
            {
                if (bytes + user.remoteSocket.Available < 2096)
                {
                    bytes += user.remoteSocket.Receive(buffer, bytes, user.remoteSocket.Available, SocketFlags.None);
                }
                else
                {
                    bytes += user.remoteSocket.Receive(buffer, bytes, (2096 - bytes), SocketFlags.None);
                    break;
                }
                if ((user.remoteSocket.Poll(10000, SelectMode.SelectRead) && user.remoteSocket.Available == 0) || !user.remoteSocket.Connected)
                {
                    user.enable = false;
                    return null;
                }
            }
            while (user.remoteSocket.Available > 0);
            request = Encoding.UTF8.GetString(buffer, 0, bytes);
            return request;
        }
        public void EndConnect(ref ChatRoom room, ref ChatUser user)
        {
            if (user.remoteSocket != null)
            {
                if (user.remoteSocket.Connected)
                {
                    user.remoteSocket.Close();
                }
            }
            room.users.Remove(user);
            Logger.WriteLog("User was removed from chat.", LogLevel.Usual);
        }
        public dynamic GetProtocolParameter(ref string request, int number, TypeCode type)
        {
            int end = 0;
            int start = 0;
            string answer = null;
            for (int i = -1; i < number; i++)
            {
                end = request.IndexOf(':', start);
                if (end == -1)
                {
                    break;
                }
                if (i + 1 == number)
                {
                    answer = request.Substring(start, end - start);
                    break;
                }
                start = end + 1;
            }
            if (answer != null)
            {
                switch (type)
                {
                    case TypeCode.Int32:
                        int intValue = -1;
                        if (Int32.TryParse(answer, out intValue)) { return intValue; }
                        else 
                        { 
                            return -1; 
                        }
                    default:
                        Logger.WriteLog("Can't convert save string, method was't able to convert value in this type", LogLevel.Warning);
                        return null;
                }
            }
            else
            {
                Logger.WriteLog("Can't get protocol patameter value, number->" + number, LogLevel.Usual);
                return null;
            }
        }
        public void HandleMessage(ref string request, ref ChatUser user)
        {
            Message message = new Message();
            message.message_text = GetProtocolParameter(ref request, 3, TypeCode.String);
            if (message.message_text != null)
            {
                message.message_id = user.chat_room.message_count;
                message.user_id = user.user_id;
                message.user_login = user.user_login;
                ++user.chat_room.message_count;
                SendToChat(message.user_id + ":\n" + message.message_text,ref user.chat_room);
                Logger.WriteLog("Message was handled, message_id->" + message.message_id + " chat.chat_id->" + user.chat_room.chat_id, LogLevel.Usual);
            }
        }
        public void SendToChat(string message, ref ChatRoom chat)
        {
            foreach (ChatUser user in chat.users)
            {
                SendSocket(ref user.remoteSocket, ref message);
            }
        }
    }
}

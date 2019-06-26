using System;
using System.Text;
using Common.Logging;
using Common.NDatabase;
using System.Net.Sockets;
using System.Diagnostics;
using Common.Functional.Pass;
using Common.NDatabase.UserData;
using System.Collections.Generic;
using MiniMessanger.Models.Chat;
using Newtonsoft.Json;

namespace Common.Chats
{
    public class ChatModule
    {
        public bool request_view = false;

        private Dictionary<int, ChatRoom> rooms = new Dictionary<int, ChatRoom>();

        public void IdentifyChatUser(ref Socket remoteSocket, ref byte[] buffer, ref int bytes)
        {
            string message = null;
            string connection = Encoding.UTF8.GetString(buffer, 0, bytes);
            ChatRoom room = new ChatRoom();
            room.users = new List<ChatUser>();
            Debug.WriteLine(connection);
            if (request_view)
            {
                Console.WriteLine(connection);
            }
            string user_token = GetProtocolParameter(ref connection, 0, TypeCode.String);
            UserCache user = new UserCache();
            if (Database.user.SelectUserByToken(user_token, ref user))
            {
                string opposide_public_token = GetProtocolParameter(ref connection, 1, TypeCode.String);
                UserCache interlocutor = new UserCache();
                if (Database.user.SelectUserByPublicToken(opposide_public_token, ref interlocutor))
                {
                    Participant participant = new Participant();
                    if (!Database.participant.SelectByUserOpposideId(user.user_id, interlocutor.user_id, ref participant))
                    {
                        room.chat_token = Validator.GenerateHash(20);
                        room.created_at = DateTime.Now;
                        Database.chat.AddChat(ref room);
                        Participant user_participant = new Participant();
                        user_participant.chat_id = room.chat_id;
                        user_participant.user_id = user.user_id;
                        user_participant.opposide_id = interlocutor.user_id;
                        Database.participant.AddParticipant(ref user_participant);
                        Participant opposide_participant = new Participant();
                        opposide_participant.chat_id = room.chat_id;
                        opposide_participant.user_id = interlocutor.user_id;
                        opposide_participant.opposide_id = user.user_id;
                        Database.participant.AddParticipant(ref opposide_participant);
                        rooms.Add(room.chat_id, room);
                    }
                    else 
                    {
                        Database.chat.SelectChatById(participant.chat_id, ref room);
                        if (!rooms.ContainsKey(room.chat_id))
                        {
                            rooms.Add(room.chat_id, room);
                        }
                    }
                    ChatUser chatUser = new ChatUser();
                    chatUser.user_id = user.user_id;
                    chatUser.enable = true;
                    chatUser.remoteSocket = remoteSocket;
                    chatUser.chat_id = room.chat_id;
                    rooms[room.chat_id].users.Add(chatUser);
                    Logger.WriteLog("Start Processing user.user_id->" + user.user_id + ".", LogLevel.Usual);
                    HandleMessage(ref connection, ref chatUser, 2);
                    Processing(ref chatUser);
                    return;
                }
                else { message = "Can't define interlocutor by interlocutor_public_token from connection request's body."; }
            }
            else { message = "Can't define user by user_token from connection request's body."; }
            Logger.WriteLog(message, LogLevel.Warning);
            SendSocket(ref remoteSocket, ref message);
        }
        public void Processing(ref ChatUser user)
        {
            for (; ; )
            {
                string request = ReceiveMessage(ref user);
                if (!user.enable)
                {
                    EndConnect(ref user);
                    break;
                }
                if (!string.IsNullOrEmpty(request))
                {
                    HandleMessage(ref request, ref user, 1);
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
                if ((user.remoteSocket.Poll(10000, SelectMode.SelectRead) && user.remoteSocket.Available == 0) || !user.remoteSocket.Connected)
                {
                    user.enable = false;
                    return null;
                }
                if (bytes + user.remoteSocket.Available < 2096)
                {
                    bytes += user.remoteSocket.Receive(buffer, bytes, user.remoteSocket.Available, SocketFlags.None);
                }
                else
                {
                    bytes += user.remoteSocket.Receive(buffer, bytes, (2096 - bytes), SocketFlags.None);
                    break;
                }
            }
            while (user.remoteSocket.Available > 0);
            request = Encoding.UTF8.GetString(buffer, 0, bytes);
            return request;
        }
        public void EndConnect(ref ChatUser user)
        {
            if (user.remoteSocket != null)
            {
                if (user.remoteSocket.Connected)
                {
                    user.remoteSocket.Close();
                }
            }
            rooms[user.chat_id].users.Remove(user);
            if (rooms.Count < 0)
            {
                rooms.Remove(user.chat_id);
                Logger.WriteLog("Remove chat from dictionary of rooms.", LogLevel.Usual);
            }
            Logger.WriteLog("User.user_id-> " + user.user_id + " was removed from chat.", LogLevel.Usual);
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
                    case TypeCode.Int32: int intValue = -1;
                        Int32.TryParse(answer, out intValue);
                        return intValue;
                    case TypeCode.String:
                        return answer;
                    default:
                        Logger.WriteLog("Can't convert save string, method was't able to convert value in this type", LogLevel.Warning);
                        return null;
                }
            }
            else
            {
                Logger.WriteLog("Can't get protocol patameter value, number->" + number, LogLevel.Usual);
                switch (type)
                {
                    case TypeCode.Int32: return -1;
                    case TypeCode.String: return null;
                    default: return null;
                }
            }
        }
        public void HandleMessage(ref string request, ref ChatUser user, int position_message)
        {
            Debug.WriteLine(request);
            if (request_view)
            {
                Console.WriteLine(request);
            }
            Message message = new Message();
            message.chat_id = user.chat_id;
            message.user_id = user.user_id;
            message.message_text = GetProtocolParameter(ref request, position_message, TypeCode.String);
            message.message_viewed = false;
            message.created_at = DateTime.Now;
            if (!string.IsNullOrEmpty(message.message_text))
            {
                SendToChat(ref message, ref user.chat_id);
                Database.message.AddMessage(ref message);
                Logger.WriteLog("Message was handled, message_id->" + message.message_id + " chat.chat_id->" + user.chat_id, LogLevel.Usual);
            }
        }
        public void HandleMessage(ref string message_text, int chat_id, int user_id)
        {
            Message message = new Message();
            message.chat_id = chat_id;
            message.user_id = user_id;
            message.message_text = message_text;
            message.message_viewed = false;
            message.created_at = DateTime.Now;
            Database.message.AddMessage(ref message);
            Logger.WriteLog("Message was handled, message_id->" + message.message_id + " chat.chat_id->" + chat_id, LogLevel.Usual);
        }

        public void SendToChat(ref Message message, ref int chat_id)
        {
            if (rooms.ContainsKey(chat_id))
            {
                foreach (ChatUser user in rooms[chat_id].users)
                {
                    string message_json = JsonConvert.SerializeObject(message); 
                    SendSocket(ref user.remoteSocket, ref message_json);
                    if (user.user_id != message.user_id)
                    {
                        message.message_viewed = true;
                    }
                }
            }
            Logger.WriteLog("Handle message to chat, chat_id->" + chat_id + ".", LogLevel.Usual);
        }
    }
}

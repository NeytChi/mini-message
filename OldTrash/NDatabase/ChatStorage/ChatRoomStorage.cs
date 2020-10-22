using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Common.Chats;
using Common.NDatabase;
using MySql.Data.MySqlClient;

namespace MiniMessanger.NDatabase.ChatStorage
{
    public class ChatRoomStorage : Storage
    {
        public ChatRoomStorage(MySqlConnectionStringBuilder connectionstring)
        {
            this.connectionstring = connectionstring;
            SetTableName("chatroom");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS chatroom" +
                "(" +
                    "chat_id int NOT NULL AUTO_INCREMENT, " +
                    "chat_token varchar(20), " +
                    "created_at timestamp, " +
                    "PRIMARY KEY (chat_id) " +
                ");"
            );
        }
        public void AddChat(ref ChatRoom chat)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO chatroom(chat_token, created_at)" +
                "VALUES (@chat_token, @created_at);", connection))
                {
                    commandSQL.Parameters.AddWithValue("@chat_token", chat.chat_token);
                    commandSQL.Parameters.AddWithValue("@created_at", chat.created_at.ToString("yyyy/MM/dd/ HH:mm:ss"));
                    commandSQL.ExecuteNonQuery();
                    chat.chat_id = (int)commandSQL.LastInsertedId;
                }
            }
            Logger.WriteLog("Add chat.chat_id->" + chat.chat_id + " to database.", LogLevel.Usual);
        }
        public bool SelectChatById(int chat_id, ref ChatRoom room)
        {
            bool success = false;
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM chatroom WHERE chat_id=@chat_id;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@chat_id", chat_id);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        if (readerMassive.Read())
                        {
                            room.chat_id = readerMassive.GetInt32(0);
                            room.chat_token = readerMassive.GetString(1);
                            room.created_at = readerMassive.GetDateTime(2);
                            success = true;
                        }
                    }
                }
            }
            Logger.WriteLog("Select chat by chat_id. Success->" + success, LogLevel.Usual);
            return success;
        }
        public bool SelectChatByToken(ref string chat_token, ref ChatRoom room)
        {
            bool success = false;
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM chatroom WHERE chat_token=@chat_token;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@chat_token", chat_token);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        if (readerMassive.Read())
                        {
                            room.chat_id = readerMassive.GetInt32(0);
                            room.chat_token = readerMassive.GetString(1);
                            room.created_at = readerMassive.GetDateTime(2);
                            success = true;
                        }
                    }
                }
            }
            Logger.WriteLog("Select chat by chat_token. Success->" + success, LogLevel.Usual);
            return success;
        }
    }
}
            

using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Common.Chats;
using Common.NDatabase;
using MySql.Data.MySqlClient;

namespace MiniMessanger.NDatabase.ChatStorage
{
    public class ChatsStorage : Storage
    {
        public ChatsStorage(MySqlConnection connection, Semaphore s_locker)
        {
            this.connection = connection;
            this.s_locker = s_locker;
            SetTableName("chats");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS chats" +
                "(" +
                    "chat_id long NOT NULL AUTO_INCREMENT," +
                    "creator_id int," +
                    "created_at datetime," +
                    "PRIMARY KEY (chat_id)" +
                ");"
            );
        }
        public void AddChat(ref ChatRoom chat)
        {
            using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO chats(creator_id, created_at)" +
                "VALUES (@creator_id, @created_at);", connection))
            {
                commandSQL.Parameters.AddWithValue("@creator_id", chat.creator_id);
                commandSQL.Parameters.AddWithValue("@created_at", chat.created_at);
                s_locker.WaitOne();
                commandSQL.ExecuteNonQuery();
                chat.chat_id = (int)commandSQL.LastInsertedId;
                commandSQL.Dispose();
                s_locker.Release();
            }
            Logger.WriteLog("Add chat.chat_id->" + chat.chat_id + " to database.", LogLevel.Usual);
        }
        public List<ChatRoom> SelectByUserId(int user_id)
        {
            List<ChatRoom> messages = new List<ChatRoom>();
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM chats WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    while (readerMassive.Read())
                    {
                        ChatRoom chat = new ChatRoom();
                        chat.chat_id = readerMassive.GetInt32(0);
                        chat.creator_id = readerMassive.GetInt32(1);
                        chat.created_at = readerMassive.GetDateTime(2);
                    }
                }
                s_locker.Release();
            }
            Logger.WriteLog("Select chats by user_id->" + user_id + ".", LogLevel.Usual);
            return messages;
        }
    }
}
            

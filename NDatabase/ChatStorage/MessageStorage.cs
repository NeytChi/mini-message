using System;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Common.Chats;
using Common.NDatabase;
using MySql.Data.MySqlClient;

namespace MiniMessanger.NDatabase.ChatStorage
{
    public class MessageStorage : Storage
    {
        public MessageStorage(MySqlConnectionStringBuilder connectionstring)
        {
            this.connectionstring = connectionstring;
            SetTableName("messages");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS messages" +
                "(" +
                    "message_id bigint NOT NULL AUTO_INCREMENT," +
                    "chat_id int," +
                    "user_id int," +
                    "message_text varchar(500)  CHARACTER SET utf8 COLLATE utf8_general_ci," +
                    "message_viewed bool," +
                    "created_at timestamp," +
                    "PRIMARY KEY (message_id)" +
                ");"
            );
        }
        public void AddMessage(ref Message message)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO messages(chat_id, user_id, message_text, message_viewed, created_at)" +
                "VALUES (@chat_id, @user_id, @message_text, @message_viewed, @created_at);", connection))
                {
                    commandSQL.Parameters.AddWithValue("@chat_id", message.chat_id);
                    commandSQL.Parameters.AddWithValue("@user_id", message.user_id);
                    commandSQL.Parameters.AddWithValue("@message_text", message.message_text);
                    commandSQL.Parameters.AddWithValue("@message_viewed", message.message_viewed);
                    commandSQL.Parameters.AddWithValue("@created_at", message.created_at.ToString("yyyy/MM/dd/ HH:mm:ss"));
                    commandSQL.ExecuteNonQuery();
                    message.message_id = (int)commandSQL.LastInsertedId;
                    commandSQL.Dispose();
                }
            }
            Logger.WriteLog("Add message.message_id->" + message.message_id + " to database.", LogLevel.Usual);
        }
        public List<Message> SelectMessageByChatId(int chat_id, int since, int count)
        {
            List<Message> messages = new List<Message>();
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM messages WHERE chat_id=@chat_id ORDER BY message_id DESC LIMIT @since, @count;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@chat_id", chat_id);
                    commandSQL.Parameters.AddWithValue("@since", since);
                    commandSQL.Parameters.AddWithValue("@count", count);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        while (readerMassive.Read())
                        {
                            Message message = new Message();
                            message.message_id = readerMassive.GetInt64(0);
                            message.chat_id = readerMassive.GetInt64(1);
                            message.user_id = readerMassive.GetInt32(2);
                            message.message_text = readerMassive.GetString(3);
                            message.message_viewed = readerMassive.GetBoolean(4);
                            message.created_at = readerMassive.GetDateTime(5);
                            messages.Add(message);
                        }
                    }
                }
            }
            Logger.WriteLog("Select messages by chat_id->" + chat_id + " since->" + since + ", count->" + count + ".", LogLevel.Usual);
            return messages;
        }
        public Message SelectLastMessage(int chat_id)
        {
            Message message = new Message();
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM messages WHERE chat_id=@chat_id ORDER BY message_id DESC LIMIT 1;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@chat_id", chat_id);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        if (readerMassive.Read())
                        {
                            message.message_id = readerMassive.GetInt64(0);
                            message.chat_id = readerMassive.GetInt64(1);
                            message.user_id = readerMassive.GetInt32(2);
                            message.message_text = readerMassive.GetString(3);
                            message.message_viewed = readerMassive.GetBoolean(4);
                            message.created_at = readerMassive.GetDateTime(5);
                        }
                    }
                }
            }
            Logger.WriteLog("Select last message by chat_id->" + chat_id + ".", LogLevel.Usual);
            return message;
        }
        public bool SelectMessage(long message_id, ref Message message)
        {
            bool success = false;
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM messages WHERE message_id=@message_id;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@message_id", message_id);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        if (readerMassive.Read())
                        {
                            success = true;
                            message.message_id = readerMassive.GetInt64(0);
                            message.chat_id = readerMassive.GetInt64(1);
                            message.user_id = readerMassive.GetInt32(2);
                            message.message_text = readerMassive.GetString(3);
                            message.message_viewed = readerMassive.GetBoolean(4);
                            message.created_at = readerMassive.GetDateTime(5);
                        }
                    }
                }
            }
            Logger.WriteLog("Select message by message_id->" + message_id + "; success->" + success + ".", LogLevel.Usual);
            return success;
        }
        public void UpdateMessages(int chat_id, bool message_viewed)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("UPDATE messages SET message_viewed=@message_viewed WHERE chat_id=@chat_id;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@message_viewed", message_viewed);
                    commandSQL.Parameters.AddWithValue("@chat_id", chat_id);
                    commandSQL.ExecuteNonQuery();
                    commandSQL.Dispose();
                }
            }
            Logger.WriteLog("Update message_viewed of messages.", LogLevel.Usual);
        }
    }
}


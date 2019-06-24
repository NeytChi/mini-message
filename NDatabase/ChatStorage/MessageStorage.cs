﻿using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Common.Chats;
using Common.NDatabase;
using MySql.Data.MySqlClient;

namespace MiniMessanger.NDatabase.ChatStorage
{
    public class MessageStorage : Storage
    {
        public MessageStorage(MySqlConnection connection, Semaphore s_locker)
        {
            this.connection = connection;
            this.s_locker = s_locker;
            SetTableName("messages");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS messages" +
                "(" +
                    "message_id bigint NOT NULL AUTO_INCREMENT," +
                    "chat_id int," +
                    "user_id int," +
                    "message_text varchar(500)," +
                    "message_viewed bool," +
                    "created_at datetime," +
                    "PRIMARY KEY (message_id)" +
                ");"
            );
        }
        public void AddMessage(ref Message message)
        {
            using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO messages(chat_id, user_id, message_text, message_viewed, created_at)" +
                "VALUES (@chat_id, @user_id, @message_text, @message_viewed, @created_at);", connection))
            {
                commandSQL.Parameters.AddWithValue("@chat_id", message.chat_id);
                commandSQL.Parameters.AddWithValue("@user_id", message.user_id);
                commandSQL.Parameters.AddWithValue("@message_text", message.message_text);
                commandSQL.Parameters.AddWithValue("@message_viewed", message.message_viewed);
                commandSQL.Parameters.AddWithValue("@created_at", message.created_at);
                s_locker.WaitOne();
                commandSQL.ExecuteNonQuery();
                message.message_id = (int)commandSQL.LastInsertedId;
                commandSQL.Dispose();
                s_locker.Release();
            }
            Logger.WriteLog("Add message.message_id->" + message.message_id + " to database.", LogLevel.Usual);
        }
        public List<Message> SelectMessageByChatId(int chat_id, int since, int count)
        {
            List<Message> messages = new List<Message>();
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM messages WHERE chat_id=@chat_id ORDER BY message_id DESC LIMIT @since, @count;", connection))
            {
                commandSQL.Parameters.AddWithValue("@chat_id", chat_id);
                commandSQL.Parameters.AddWithValue("@since", since);
                commandSQL.Parameters.AddWithValue("@count", count);
                s_locker.WaitOne();
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
                s_locker.Release();
            }
            Logger.WriteLog("Select messages by chat_id->" + chat_id + " since->" + since + ", count->" + count + ".", LogLevel.Usual);
            return messages;
        }
        public Message SelectLastMessage(int chat_id)
        {
            Message message = new Message();
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM messages WHERE chat_id=@chat_id ORDER BY message_id DESC LIMIT 1;", connection))
            {
                commandSQL.Parameters.AddWithValue("@chat_id", chat_id);
                s_locker.WaitOne();
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
                s_locker.Release();
            }
            Logger.WriteLog("Select last message by chat_id->" + chat_id + ".", LogLevel.Usual);
            return message;
        }
        public void UpdateMessages(int chat_id, bool message_viewed)
        {
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("UPDATE messages SET message_viewed=@message_viewed WHERE chat_id=@chat_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@message_viewed", message_viewed);
                commandSQL.Parameters.AddWithValue("@chat_id", chat_id);
                commandSQL.ExecuteNonQuery();
                commandSQL.Dispose();
            }
            s_locker.Release();
            Logger.WriteLog("Update message_viewed of messages.", LogLevel.Usual);
        }
    }
}


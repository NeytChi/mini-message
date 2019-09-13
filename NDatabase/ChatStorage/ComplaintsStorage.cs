using System;
using System.Collections.Generic;
using Common.Logging;
using Common.NDatabase;
using Common.NDatabase.UserData;
using MiniMessanger.Models.Chat;
using MySql.Data.MySqlClient;
namespace MiniMessanger.NDatabase.ChatStorage
{
    public class ComplaintsStorage : Storage
    {
        public ComplaintsStorage(MySqlConnectionStringBuilder connectionstring)
        {
            this.connectionstring = connectionstring;
            SetTableName("complaints");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS complaints" +
                "(" +
                    "complaint_id int NOT NULL AUTO_INCREMENT, " +
                    "user_id int NOT NULL, " +
                    "blocked_id int NOT NULL, " +
                    "message_id bigint NOT NULL, " +
                    "complaint varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci, " +
                    "created_at DATETIME NOT NULL, " +
                    "PRIMARY KEY (complaint_id), " +
                    "FOREIGN KEY (user_id) REFERENCES users(user_id)," +
                    "FOREIGN KEY (blocked_id) REFERENCES blocked_users(blocked_id)," +
                    "FOREIGN KEY (message_id) REFERENCES messages(message_id)" +
                ");"
            );
        }
        public void Add(ref Complaint complaint)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO complaints(user_id, blocked_id, message_id, complaint, created_at) " +
                "VALUES (@user_id, @blocked_id, @message_id, @complaint, @created_at);", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", complaint.user_id);
                    commandSQL.Parameters.AddWithValue("@blocked_id", complaint.blocked_id);
                    commandSQL.Parameters.AddWithValue("@message_id", complaint.message_id);
                    commandSQL.Parameters.AddWithValue("@complaint", complaint.complaint);
                    commandSQL.Parameters.AddWithValue("@created_at", complaint.created_at);
                    commandSQL.ExecuteNonQuery();
                    complaint.complaint_id = (int)commandSQL.LastInsertedId;
                }
            }
            Logger.WriteLog("Add complaint; user.user_id->" + complaint.user_id + "; complaint_id->" + complaint.complaint_id + ".", LogLevel.Usual);
        }
        public bool CheckComplainedUser(int user_id, int blocked_user_id)
        {
            bool success = false;
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT blocked_id FROM blocked_users WHERE user_id=@user_id AND blocked_user_id=@blocked_user_id AND blocked_users.blocked_deleted=false;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", user_id);
                    commandSQL.Parameters.AddWithValue("@blocked_user_id", blocked_user_id);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        if (readerMassive.Read())
                        {
                            success = true;
                        }
                    }
                }
            }
            Logger.WriteLog("Check blocked user; user_id->" + user_id + "; blocked_user_id->" + blocked_user_id + "; success->" + success + ".", LogLevel.Usual);
            return success;
        }
    }
}
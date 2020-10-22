using System;
using System.Collections.Generic;
using Common.Logging;
using Common.NDatabase;
using Common.NDatabase.UserData;
using MiniMessanger.Models.Chat;
using MySql.Data.MySqlClient;

namespace MiniMessanger.NDatabase.ChatStorage
{
    public class BlockedUserStorage : Storage
    {
        public BlockedUserStorage(MySqlConnectionStringBuilder connectionstring)
        {
            this.connectionstring = connectionstring;
            SetTableName("blocked_users");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS blocked_users" +
                "(" +
                    "blocked_id int NOT NULL AUTO_INCREMENT, " +
                    "user_id int NOT NULL, " +
                    "blocked_user_id int NOT NULL, " +
                    "blocked_reason varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci, " +
                    "blocked_deleted boolean, " +
                    "PRIMARY KEY (blocked_id), " +
                    "FOREIGN KEY (user_id) REFERENCES users(user_id)," +
                    "FOREIGN KEY (blocked_user_id) REFERENCES users(user_id)" +
                ");"
            );
        }
        public void Add(ref BlockedUser blocked)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO blocked_users(user_id, blocked_user_id, blocked_reason, blocked_deleted)" +
                "VALUES (@user_id, @blocked_user_id, @blocked_reason, @blocked_deleted);", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", blocked.user_id);
                    commandSQL.Parameters.AddWithValue("@blocked_user_id", blocked.blocked_user_id);
                    commandSQL.Parameters.AddWithValue("@blocked_reason", blocked.blocked_reason);
                    commandSQL.Parameters.AddWithValue("@blocked_deleted", blocked.blocked_deleted);
                    commandSQL.ExecuteNonQuery();
                    blocked.blocked_id = (int)commandSQL.LastInsertedId;
                }
            }
            Logger.WriteLog("Add blocked user; user.user_id->" + blocked.user_id + "; blocked_user_id->" + blocked.blocked_user_id + ".", LogLevel.Usual);
        }
        public List<dynamic> SelectBlockedUsers(int user_id)
        {
            List<dynamic> users = new List<dynamic>();
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand(
                "SELECT users.user_email, users.user_login, users.last_login_at, users.user_public_token, blocked_users.blocked_reason " +
                "FROM blocked_users INNER JOIN users ON blocked_users.blocked_user_id=users.user_id WHERE blocked_users.user_id=@user_id AND blocked_users.blocked_deleted=false;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", user_id);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        while (readerMassive.Read())
                        {
                            var blockedUser = new 
                            {
                                user_email = readerMassive.GetString(0),
                                user_login = readerMassive.GetString(1),
                                last_login_at = readerMassive.GetInt32(2),
                                user_public_token = readerMassive.GetString(3),
                                blocked_reason = readerMassive.GetString(4)
                            };
                            users.Add(blockedUser);
                        }
                    }
                }
            }
            Logger.WriteLog("Select blocked users; user_id->" + user_id + ".", LogLevel.Usual);
            return users;
        }
        public void GetNonBlockedUsers(int user_id, ref List<UserCache> users)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT blocked_user_id FROM blocked_users WHERE user_id=@user_id AND blocked_deleted=false;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", user_id);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        while (readerMassive.Read())
                        {
                            foreach (UserCache user in users)
                            {
                                if (user.user_id == readerMassive.GetInt32(0))
                                {
                                    users.Remove(user);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            Logger.WriteLog("Get non-blocked user; user_id->" + user_id + ".", LogLevel.Usual);
        }
        public void GetNonBlockedUsers(int user_id, ref List<Participant> users)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT blocked_user_id FROM blocked_users WHERE user_id=@user_id AND blocked_deleted=false;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", user_id);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        while (readerMassive.Read())
                        {
                            foreach (Participant user in users)
                            {
                                if (user.opposide_id == readerMassive.GetInt32(0))
                                {
                                    users.Remove(user);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            Logger.WriteLog("Get non-blocked user; user_id->" + user_id + ".", LogLevel.Usual);
        }
        public bool CheckBlockedUser(int user_id, int blocked_user_id)
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
        public void DeleteBlockedUser(int user_id, int blocked_user_id)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("UPDATE blocked_users SET blocked_users.blocked_deleted=true WHERE user_id=@user_id AND blocked_user_id=@blocked_user_id AND blocked_users.blocked_deleted=false;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", user_id);
                    commandSQL.Parameters.AddWithValue("@blocked_user_id", blocked_user_id);
                    commandSQL.ExecuteNonQuery();
                    commandSQL.Dispose();
                }
            }
            Logger.WriteLog("Delete blocked user; user.user_id->" + user_id + "; blocked_user_id->" + blocked_user_id + ".", LogLevel.Usual);
        }
    }
}
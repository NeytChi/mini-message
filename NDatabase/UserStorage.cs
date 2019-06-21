using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using MySql.Data.MySqlClient;

namespace Common.NDatabase.UserData
{
    public class UserStorage : Storage
    {
        public UserStorage(MySqlConnection connection, Semaphore s_locker)
        {
            this.connection = connection;
            this.s_locker = s_locker;
            SetTableName("users");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS users" +
                "(" +
                    "user_id int NOT NULL AUTO_INCREMENT," +
                    "user_email varchar(256) UNIQUE," +
                    "user_login varchar(256)," +
                    "user_password varchar(256)," +
                    "created_at int," +
                    "user_hash varchar(120)," +
                    "activate tinyint DEFAULT '0'," +
                    "user_type varchar(25)," +
                    "user_token varchar(50), " +
                    "last_login_at int," +
                    "recovery_code int," +
                    "recovery_token varchar(50), " +
                    "PRIMARY KEY (user_id)" +
                ");"
            );
        }
        public void AddUser(ref UserCache user)
        {
            using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO users(user_email, user_login, user_password, created_at, user_hash, activate, user_token, last_login_at,recovery_code ,recovery_token)" +
                "VALUES (@user_email, @user_login, @user_password, @created_at, @user_hash, @activate, @user_token, @last_login_at, @recovery_code, @recovery_token);", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_email", user.user_email);
                commandSQL.Parameters.AddWithValue("@user_login", user.user_login);
                commandSQL.Parameters.AddWithValue("@user_password", user.user_password);
                commandSQL.Parameters.AddWithValue("@created_at", user.created_at);
                commandSQL.Parameters.AddWithValue("@user_hash", user.user_hash);
                commandSQL.Parameters.AddWithValue("@activate", user.activate);
                commandSQL.Parameters.AddWithValue("@user_token", user.user_token);
                commandSQL.Parameters.AddWithValue("@last_login_at", user.last_login_at);
                commandSQL.Parameters.AddWithValue("@recovery_code", user.recovery_code);       
                commandSQL.Parameters.AddWithValue("@recovery_token", user.recovery_token);
                s_locker.WaitOne();
                commandSQL.ExecuteNonQuery();
                user.user_id = (int)commandSQL.LastInsertedId;
                user.user_password = null;
                commandSQL.Dispose();
                s_locker.Release();
            }
            Logger.WriteLog("Add user.user_id->" + user.user_id + " to database.", LogLevel.Usual);
        }
        public bool SelectUserById(int user_id, ref UserCache user)
        {
            bool answer = false;
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM users WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    if (readerMassive.Read())
                    {
                        user.user_id = readerMassive.GetInt32("user_id");
                        user.user_email = readerMassive.GetString("user_email");
                        user.user_login = readerMassive.GetString("user_login");
                        user.user_password = readerMassive.GetString("user_password");
                        user.created_at = readerMassive.GetInt32("created_at");
                        user.user_hash = readerMassive.GetString("user_hash");
                        user.activate = readerMassive.GetInt16("activate");
                        user.user_token = readerMassive.GetString("user_token");
                        user.last_login_at = readerMassive.GetInt32("last_login_at");
                        user.recovery_code = readerMassive.GetInt32("recovery_code");
                        user.recovery_token = readerMassive.GetString("recovery_token");
                        s_locker.Release();
                        Logger.WriteLog("Select user by user_id. user.user_id->" + user_id, LogLevel.Usual);
                        answer = true;
                    }
                    else
                    {
                        s_locker.Release();
                        Logger.WriteLog("Can not select user by user_id.", LogLevel.Warning);
                        answer = false;
                    }
                }
            }
            return answer;
        }
        public bool SelectUserById(string user_token, ref UserCache user)
        {
            bool answer = false;
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM users WHERE user_token=@user_token;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_token", user_token);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    if (readerMassive.Read())
                    {
                        user.user_id = readerMassive.GetInt32("user_id");
                        user.user_email = readerMassive.GetString("user_email");
                        user.user_login = readerMassive.GetString("user_login");
                        user.user_password = readerMassive.GetString("user_password");
                        user.created_at = readerMassive.GetInt32("created_at");
                        user.user_hash = readerMassive.GetString("user_hash");
                        user.activate = readerMassive.GetInt16("activate");
                        user.user_token = readerMassive.GetString("user_token");
                        user.last_login_at = readerMassive.GetInt32("last_login_at");
                        user.recovery_code = readerMassive.GetInt32("recovery_code");
                        user.recovery_token = readerMassive.IsDBNull(11) ? null : readerMassive.GetString("recovery_token");
                        s_locker.Release();
                        answer = true;
                        Logger.WriteLog("Select user by user_token. user.user_token->" + user_token + ".", LogLevel.Usual);
                    }
                    else
                    {
                        s_locker.Release();
                        Logger.WriteLog("Can't select user by user_token.", LogLevel.Warning);
                        answer = false;
                    }
                }
            }
            return answer;
        }
        public bool SelectUserByEmail(string user_email, ref UserCache user)
        {
            bool answer = false;
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM users WHERE user_email=@user_email;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_email", user_email);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    if (readerMassive.Read())
                    {
                        user.user_id = readerMassive.GetInt32("user_id");
                        user.user_email = readerMassive.GetString("user_email");
                        user.user_login = readerMassive.GetString("user_login");
                        user.user_password = readerMassive.GetString("user_password");
                        user.created_at = readerMassive.GetInt32("created_at");
                        user.user_hash = readerMassive.GetString("user_hash");
                        user.activate = readerMassive.GetInt16("activate");
                        user.user_token = readerMassive.GetString("user_token");
                        user.last_login_at = readerMassive.GetInt32("last_login_at");
                        user.recovery_code = readerMassive.GetInt32("recovery_code");
                        user.recovery_token = readerMassive.IsDBNull(11) ? null : readerMassive.GetString("recovery_token");
                        answer = true;
                    }
                    else
                    {
                        answer = false;
                    }
                }
                s_locker.Release();
            }
            Logger.WriteLog("Select user by user_email. Success->" + answer, LogLevel.Usual);
            return answer;
        }
        public List<UserCache> SelectUsers(int since, int count)
        {
            List <UserCache> users = new List<UserCache>();
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM users ORDER BY user_id DESC LIMIT @since, @count;", connection))
            {
                commandSQL.Parameters.AddWithValue("@since", since);
                commandSQL.Parameters.AddWithValue("@count", count);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    while (readerMassive.Read())
                    {
                        UserCache user = new UserCache();
                        user.user_id = readerMassive.GetInt32("user_id");
                        user.user_email = readerMassive.GetString("user_email");
                        user.user_login = readerMassive.GetString("user_login");
                        user.created_at = readerMassive.GetInt32("created_at");
                        user.last_login_at = readerMassive.GetInt32("last_login_at");
                        users.Add(user);
                    }
                }
                s_locker.Release();
            }
            Logger.WriteLog("Select users since=" + since + ", count=" + count + ".", LogLevel.Usual);
            return users;
        }
        public bool SelectUserByToken(string user_token, ref UserCache user)
        {
            bool answer = false;
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM users WHERE user_token=@user_token;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_token", user_token);
                answer = GetUser(commandSQL, ref user);
            }
            Logger.WriteLog("Select user by user_token. Success->" + answer, LogLevel.Usual);
            return answer;
        }
        public bool SelectUserByRecoveryToken(string recovery_token, ref UserCache user)
        {
            bool answer = false;
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM users WHERE recovery_token=@recovery_token;", connection))
            {
                commandSQL.Parameters.AddWithValue("@recovery_token", recovery_token);
                answer = GetUser(commandSQL, ref user);
            }
            Logger.WriteLog("Select user by recovery_token. Success->" + answer, LogLevel.Usual);
            return answer;
        }
        private bool GetUser(MySqlCommand commandSQL, ref UserCache user)
        {
            bool success = false;
            s_locker.WaitOne();
            using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
            {
                if (readerMassive.Read())
                {
                    user.user_id = readerMassive.GetInt32("user_id");
                    user.user_email = readerMassive.GetString("user_email");
                    user.user_login = readerMassive.GetString("user_login");
                    user.user_password = readerMassive.GetString("user_password");
                    user.created_at = readerMassive.GetInt32("created_at");
                    user.user_hash = readerMassive.GetString("user_hash");
                    user.activate = readerMassive.GetInt16("activate");
                    user.user_token = readerMassive.GetString("user_token");
                    user.last_login_at = readerMassive.GetInt32("last_login_at");
                    user.recovery_code = readerMassive.GetInt32("recovery_code");
                    user.recovery_token = readerMassive.IsDBNull(11) ? null : readerMassive.GetString("recovery_token");
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            s_locker.Release();
            Logger.WriteLog("Get user from database, success->" + success, LogLevel.Usual);
            return success;
        }
        public bool CheckUserByEmail(string user_email)
        {
            bool answer = false;
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT user_id, user_email FROM users WHERE user_email=@user_email;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_email", user_email);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    if (readerMassive.Read())
                    {
                        answer = true;
                    }
                    else
                    {
                        answer = false;
                    }
                }
                s_locker.Release();
            }
            Logger.WriteLog("Check user by user_email. Answer->" + answer, LogLevel.Usual);
            return answer;
        }
        public void UpdateUserPassword(int user_id, string user_password)
        {
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("UPDATE users SET user_password=@user_password WHERE user_id=@user_id;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", user_id);
                    commandSQL.Parameters.AddWithValue("@user_password", user_password);
                    commandSQL.ExecuteNonQuery();
                    commandSQL.Dispose();
                }
            s_locker.Release();
            Logger.WriteLog("Update user password.", LogLevel.Usual);
        }
        public bool UpdateActivateUser(string user_hash)
        {
            bool success = false;
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("UPDATE users SET activate='1' WHERE user_hash=@user_hash;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_hash", user_hash);
                int updated = commandSQL.ExecuteNonQuery();
                if (updated > 0)
                {
                    success = true;
                }
                commandSQL.Dispose();
            }
            s_locker.Release();
            Logger.WriteLog("Update activate user.", LogLevel.Usual);
            return success;
        }
        public void UpdateEmail(int user_id, string user_email)
        {
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("UPDATE users SET user_email=@user_email WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                commandSQL.Parameters.AddWithValue("@user_email", user_email);
                commandSQL.ExecuteNonQuery();
                commandSQL.Dispose();
            }
            s_locker.Release();
            Logger.WriteLog("Update email user.", LogLevel.Usual);
        }
        public void UpdateLastLoginAt(int user_id, int last_login_at)
        {
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("UPDATE users SET last_login_at=@last_login_at WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                commandSQL.Parameters.AddWithValue("@last_login_at", last_login_at);
                commandSQL.ExecuteNonQuery();
                commandSQL.Dispose();
            }
            s_locker.Release();
            Logger.WriteLog("Update last_login_at user.", LogLevel.Usual);
        }
        public void UpdateUserToken(int user_id, string user_token)
        {
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("UPDATE users SET user_token=@user_token WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                commandSQL.Parameters.AddWithValue("@user_token", user_token);
                commandSQL.ExecuteNonQuery();
                commandSQL.Dispose();
            }
            s_locker.Release();
            Logger.WriteLog("Update user_token user.", LogLevel.Usual);
        }
        public void UpdateRecoveryCode(int user_id, int recovery_code)
        {
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("UPDATE users SET recovery_code=@recovery_code WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                commandSQL.Parameters.AddWithValue("@recovery_code", recovery_code);
                commandSQL.ExecuteNonQuery();
                commandSQL.Dispose();
            }
            s_locker.Release();
            Logger.WriteLog("Update recovery_code user.", LogLevel.Usual);
        }
        public void UpdateRecoveryToken(int user_id, string recovery_token)
        {
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("UPDATE users SET recovery_token=@recovery_token WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                commandSQL.Parameters.AddWithValue("@recovery_token", recovery_token);
                commandSQL.ExecuteNonQuery();
                commandSQL.Dispose();
            }
            s_locker.Release();
            Logger.WriteLog("Update recovery_token user.", LogLevel.Usual);
        }
        public void DeleteUser(int? user_id)
        {
            if (user_id == null)
            {
                return;
            }
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("DELETE FROM users WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                commandSQL.ExecuteNonQuery();
                commandSQL.Dispose();
            }
            s_locker.Release();
            Logger.WriteLog("Delete user.user_id->" + user_id + ".", LogLevel.Usual);
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using MySql.Data.MySqlClient;
using MiniMessanger.Models;

namespace Common.NDatabase.UserData
{
    public class ProfileStorage : Storage
    {
        public ProfileStorage(MySqlConnection connection, Semaphore s_locker)
        {
            this.connection = connection;
            this.s_locker = s_locker;
            SetTableName("profiles");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS profiles" +
                "(" +
                    "profile_id int NOT NULL AUTO_INCREMENT," +
                    "user_id int, " +
                    "url_photo varchar(256), " +
                    "PRIMARY KEY (profile_id)" +
                ");"
            );
        }
        public void AddProfile(ref ProfileData profile)
        {
            using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO profiles(user_id, url_photo)" +
                "VALUES (@user_id, @url_photo);", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", profile.user_id);
                commandSQL.Parameters.AddWithValue("@url_photo", profile.url_photo);
                s_locker.WaitOne();
                commandSQL.ExecuteNonQuery();
                profile.profile_id = (int)commandSQL.LastInsertedId;
                commandSQL.Dispose();
                s_locker.Release();
            }
            Logger.WriteLog("Add profile.user_id->" + profile.user_id + " to database.", LogLevel.Usual);
        }
        public bool SelectUserById(int user_id, ref ProfileData profile)
        {
            bool answer = false;
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM profiles WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    if (readerMassive.Read())
                    {
                        profile.profile_id = readerMassive.GetInt32(0);
                        profile.user_id = readerMassive.GetInt32(1);
                        profile.url_photo = readerMassive.GetString(2);
                        s_locker.Release();
                        Logger.WriteLog("Select profile by user_id. profile.user_id->" + user_id, LogLevel.Usual);
                        answer = true;
                    }
                    else
                    {
                        Logger.WriteLog("Can not select profile by user_id.", LogLevel.Warning);
                    }
                }
                s_locker.Release();
            }
            return answer;
        }
        public bool DeleteProfile(int user_id)
        {
            bool success = false;
            using (MySqlCommand commandSQL = new MySqlCommand("DELETE FROM profiles WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                s_locker.WaitOne();
                if (commandSQL.ExecuteNonQuery() > 0)
                {
                    success = true;
                }
                commandSQL.Dispose();
                s_locker.Release();
            }
            Logger.WriteLog("Delete profile.user_id->" + user_id + " from database. Success->" + success, LogLevel.Usual);
            return success;
        }
        public void UpdateUrlPhoto(int user_id, string url_photo)
        {
            s_locker.WaitOne();
            using (MySqlCommand commandSQL = new MySqlCommand("UPDATE profiles SET url_photo=@url_photo WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                commandSQL.Parameters.AddWithValue("@url_photo", url_photo);
                commandSQL.ExecuteNonQuery();
                commandSQL.Dispose();
            }
            s_locker.Release();
            Logger.WriteLog("Update profile url_photo.", LogLevel.Usual);
        }
    }
}

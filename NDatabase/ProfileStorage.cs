using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using MySql.Data.MySqlClient;
using MiniMessanger.Models;

namespace Common.NDatabase.UserData
{
    public class ProfileStorage : Storage
    {
        public ProfileStorage(MySqlConnectionStringBuilder connectionstring)
        {
            this.connectionstring = connectionstring;
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
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO profiles(user_id, url_photo)" +
                "VALUES (@user_id, @url_photo);", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", profile.user_id);
                    commandSQL.Parameters.AddWithValue("@url_photo", profile.url_photo);
                    commandSQL.ExecuteNonQuery();
                    profile.profile_id = (int)commandSQL.LastInsertedId;
                    commandSQL.Dispose();
                }
            }
            Logger.WriteLog("Add profile.user_id->" + profile.user_id + " to database.", LogLevel.Usual);
        }
        public bool SelectByUserId(int user_id, ref ProfileData profile)
        {
            bool answer = false;
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM profiles WHERE user_id=@user_id;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", user_id);
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        if (readerMassive.Read())
                        {
                            profile.profile_id = readerMassive.GetInt32(0);
                            profile.user_id = readerMassive.GetInt32(1);
                            profile.url_photo = readerMassive.IsDBNull(2) ? null : readerMassive.GetString(2);
                            answer = true;
                        }
                    }
                }
            }
            Logger.WriteLog("Select profile by user_id. profile.user_id->" + user_id + ". Success->" + answer, LogLevel.Usual);
            return answer;
        }
        public bool DeleteProfile(int user_id)
        {
            bool success = false;
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("DELETE FROM profiles WHERE user_id=@user_id;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", user_id);
                    if (commandSQL.ExecuteNonQuery() > 0)
                    {
                        success = true;
                    }
                    commandSQL.Dispose();
                }
            }
            Logger.WriteLog("Delete profile.user_id->" + user_id + " from database. Success->" + success, LogLevel.Usual);
            return success;
        }
        public void UpdateUrlPhoto(int user_id, string url_photo)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("UPDATE profiles SET url_photo=@url_photo WHERE user_id=@user_id;", connection))
                {
                    commandSQL.Parameters.AddWithValue("@user_id", user_id);
                    commandSQL.Parameters.AddWithValue("@url_photo", url_photo);
                    commandSQL.ExecuteNonQuery();
                    commandSQL.Dispose();
                }
            }
            Logger.WriteLog("Update profile url_photo.", LogLevel.Usual);
        }
    }
}

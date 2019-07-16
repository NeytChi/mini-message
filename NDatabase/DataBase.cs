using System;
using System.Data;
using Common.Logging;
using System.Threading;
using MySql.Data.MySqlClient;
using Common.NDatabase.LogData;
using Common.NDatabase.FileData;
using Common.NDatabase.UserData;
using System.Collections.Generic;
using MiniMessanger.NDatabase.ChatStorage;

namespace Common.NDatabase
{
    public static class Database
    {
        public static string defaultNameDB = "minimessanger";
        public static MySqlConnectionStringBuilder connectionstring = new MySqlConnectionStringBuilder();
        public static MySqlConnection connection;

        #region database_functional
        public static UserStorage user;
        public static ProfileStorage profile;
        public static LogStorage log;
        public static FileStorage file;

        public static ParticipantStorage participant;
        public static ChatRoomStorage chat;
        public static MessageStorage message;

        public static List<Storage> storages = new List<Storage>();
        public static Semaphore s_locker = new Semaphore(1, 1);
        #endregion

        public static void Initialization(bool DatabaseExist)
        {
            Console.WriteLine("MySQL connection...");
            if (!DatabaseExist)
            {
                CheckDatabaseExists();
            }
            GetJsonConfig();
            connection = new MySqlConnection(connectionstring.ToString());
            connection.Open();
            SetMainStorages();
            CheckingAllTables();
            Logger.WriteLog("Initilization database->" + defaultNameDB + " done.", LogLevel.Usual);
            Console.WriteLine("MySQL connected.");
        }
        public static void ConnectNew()
        {
            connection = new MySqlConnection(connectionstring.ToString());
            connection.Open();
            foreach (Storage storage in storages)
            {
                storage.SetConnection(ref connection);
            }
        }
        private static void SetMainStorages()
        {
            user = new UserStorage(connection, s_locker);
            log = new LogStorage(connection, s_locker);
            file = new FileStorage(connection, s_locker);
            profile = new ProfileStorage(connection, s_locker);
            participant = new ParticipantStorage(connection, s_locker);
            chat = new ChatRoomStorage(connection, s_locker);
            message = new MessageStorage(connection, s_locker);
            storages.Add(user);
            storages.Add(log);
            storages.Add(file);
            storages.Add(profile);
            storages.Add(chat);
            storages.Add(participant);
            storages.Add(message);
        }
        public static bool GetJsonConfig()
        {
            string Json = GetConfigDatabase();
            connectionstring.SslMode = MySqlSslMode.None;
            connectionstring.ConnectionReset = true;
            connectionstring.CharacterSet = "UTF8";
            if (Json == null)
            {
                connectionstring.Server = "localhost";
                connectionstring.Database = "phonetics";
                connectionstring.UserID = "root";
                connectionstring.Password = "root";
                defaultNameDB = "phonetics";
                return false;
            }
            else
            {
                var configJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(Json);
                connectionstring.Server = configJson["Server"].ToString();
                connectionstring.UserID = configJson["UserID"].ToString();
                connectionstring.Password = configJson["Password"].ToString();
                connectionstring.Database = configJson["Database"].ToString();
                return true;
            }
        }
        private static string GetConfigDatabase()
        {
            if (System.IO.File.Exists("database.conf"))
            {
                using (var fstream = System.IO.File.OpenRead("database.conf"))
                {
                    byte[] array = new byte[fstream.Length];
                    fstream.Read(array, 0, array.Length);
                    string textFromFile = System.Text.Encoding.Default.GetString(array);
                    fstream.Close();
                    return textFromFile;
                }
            }
            else
            {
                Console.WriteLine("Function getConfigInfoDB() doesn't get database configuration information. Server DB starting with default configuration.");
                return null;
            }
        }
        public static bool CheckingAllTables()
        {
            bool checking = true;
            foreach(Storage storage in storages)
            {
                if(!CheckTableExists(storage.table))
                {
                    checking = false;
                    Console.WriteLine("The table=" + storage.table + " didn't create.");
                }
            }
            Console.WriteLine("The specified tables created.");
            return checking;
        }
        private static bool CheckTableExists(string sqlCreateCommand)
        {
            try
            {
                using (MySqlCommand command = new MySqlCommand(sqlCreateCommand, connection))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\r\nError function CheckTableExists().\r\n{1}\r\nMessage:\r\n{0}\r\n", e.Message, sqlCreateCommand);
                return false;
            }
        }
        public static bool DropTables()
        {
            for (int i = storages.Count - 1; 0 < i + 1; i--)
            {
                string command = string.Format("DROP TABLE {0};", storages[i].table_name);
                using (MySqlCommand commandSQL = new MySqlCommand(command, connection))
                {
                    commandSQL.ExecuteNonQuery();
                    commandSQL.Dispose();
                }
                Console.WriteLine("Delete table->" + storages[i].table_name);
            }
            return true;
        }
        public static void CheckDatabaseExists()
        {
            string Json = GetConfigDatabase();
            connectionstring.SslMode = MySqlSslMode.None;
            connectionstring.ConnectionReset = true;
            if (Json == null)
            {
                connectionstring.Server = "localhost";
                connectionstring.UserID = "root";
                connectionstring.Password = "root";
            }
            else
            {
                var configJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(Json);
                connectionstring.Server = configJson["Server"].ToString();
                connectionstring.UserID = configJson["UserID"].ToString();
                connectionstring.Password = configJson["Password"].ToString();
                defaultNameDB = configJson["Database"].ToString();
            }
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            connection = new MySqlConnection(connectionstring.ToString());
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                using (MySqlCommand command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS " + defaultNameDB + ";", connection))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
            }
            connection.Close(); 
        }
        public static Storage AddStorage(Storage storage)
        {
            storage.connection = connection;
            if (!CheckTableExists(storage.table))
            {
                Console.WriteLine("The table=" + storage.table + " didn't create.");
                return null;
            }
            Logger.WriteLog("The storage->" + storage.table +" added to database.", LogLevel.Usual);
            return storage;
        }
    }
}

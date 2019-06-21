using System;
using System.Threading;
using Common.Logging;
using MySql.Data.MySqlClient;

namespace Common.NDatabase.FileData
{
    public class FileStorage : Storage
    {
        public FileStorage(MySqlConnection connection, Semaphore s_locker)
        {
            this.connection = connection;
            this.s_locker = s_locker;
            SetTableName("files");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS files" +
                "(" +
                    "file_id bigint AUTO_INCREMENT," +
                    "file_path varchar(256)," +
                    "file_name varchar(20) NOT NULL," +
                    "file_type varchar(10)," +
                    "file_extension varchar(10)," +
                    "file_last_name varchar(100)," +
                    "file_fullpath varchar(256)," +
                    "PRIMARY KEY (file_id)" +
                ");"
            );
        }
        public FileD AddFile(FileD file)
        {
            using (MySqlCommand sqlCommand = new MySqlCommand("INSERT INTO files" +
                "(file_path, file_name, file_type, file_extension, file_last_name, file_fullpath)" +
                "VALUES ( @file_path, @file_name, @file_type, @file_extension, @file_last_name, @file_fullpath)", connection))
            {
                sqlCommand.Parameters.AddWithValue("@file_path", file.file_path);
                sqlCommand.Parameters.AddWithValue("@file_name", file.file_name);
                sqlCommand.Parameters.AddWithValue("@file_type", file.file_type);
                sqlCommand.Parameters.AddWithValue("@file_extension", file.file_extension);
                sqlCommand.Parameters.AddWithValue("@file_last_name", file.file_last_name);
                sqlCommand.Parameters.AddWithValue("@file_fullpath", file.file_fullpath);
                s_locker.WaitOne();
                sqlCommand.ExecuteNonQuery();
                file.file_id = (int)sqlCommand.LastInsertedId;
                sqlCommand.Dispose();
                s_locker.Release();
            }
            Logger.WriteLog("Add new file.file_id->" + file.file_id + " to database.", LogLevel.Usual);
            return file;
        }
        public bool SelectById(int file_id, ref FileD file)
        {
            bool answer = false;
            using (MySqlCommand sqlCommand = new MySqlCommand("SELECT * FROM files WHERE file_id=@file_id", connection))
            {
                sqlCommand.Parameters.AddWithValue("@file_id", file_id);
                s_locker.WaitOne();
                using (MySqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        file.file_id = reader.GetInt32(0);
                        file.file_path = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        file.file_name = reader.GetString(2);
                        file.file_type = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        file.file_extension = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        file.file_last_name = reader.IsDBNull(5) ? "" : reader.GetString(5);
                        file.file_fullpath = reader.IsDBNull(6) ? "" : reader.GetString(6);
                        answer = true;
                    }
                    else
                    {
                        answer = false;
                    }
                }
                s_locker.Release();
            }
            Logger.WriteLog("Select file by file_id success->" + answer, LogLevel.Usual);
            return answer;
        }
        public bool SelectByFullPath(string file_fullpath, ref FileD file)
        {
            bool answer = false;
            using (MySqlCommand sqlCommand = new MySqlCommand("SELECT * FROM files WHERE file_fullpath=@file_fullpath", connection))
            {
                sqlCommand.Parameters.AddWithValue("@file_fullpath", file_fullpath);
                s_locker.WaitOne();
                using (MySqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        file.file_id = reader.GetInt32(0);
                        file.file_path = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        file.file_name = reader.GetString(2);
                        file.file_type = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        file.file_extension = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        file.file_last_name = reader.IsDBNull(5) ? "" : reader.GetString(5);
                        file.file_fullpath = reader.IsDBNull(6) ? "" : reader.GetString(6);
                        answer = true;
                    }
                    else
                    {
                        answer = false;
                    }
                }
                s_locker.Release();
            }
            Logger.WriteLog("Select file by file_id success->" + answer, LogLevel.Usual);
            return answer;
        }
        public bool DeleteById(int file_id)
        {
            int deleted = 0;
            using (MySqlCommand sqlCommand = new MySqlCommand("DELETE FROM files WHERE file_id=@file_id", connection))
            {
                sqlCommand.Parameters.AddWithValue("@file_id", file_id);
                s_locker.WaitOne();
                deleted = sqlCommand.ExecuteNonQuery();
                s_locker.Release();
                if (deleted != 0)
                {
                    Logger.WriteLog("File.file_id->" + file_id + " delete is success->" + true + ".", LogLevel.Usual);
                    return true;
                }
                else
                {
                    Logger.WriteLog("File.file_id->" + file_id + " delete is success->" + false + ".", LogLevel.Usual);
                    return false;
                }
            }
        }
    }
}
























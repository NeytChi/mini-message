using Common.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Common.NDatabase.LogData
{
    public class LogStorage : Storage
    {
        public LogStorage(MySqlConnectionStringBuilder connectionstring)
        {
            this.connectionstring = connectionstring;
            SetTableName("logs");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS logs" +
                "(" +
                    "log_id bigint AUTO_INCREMENT," +
                    "log text(2000)," +
                    "user_computer text(100)," +
                    "seconds varchar(10)," +
                    "minutes varchar(10)," +
                    "hours varchar(10)," +
                    "day varchar(10)," +
                    "month varchar(10)," +
                    "year varchar(10)," +
                    "level varchar(100)," +
                    "PRIMARY KEY (log_id)" +
                ");"
            );
        }
        public void AddLogs(ref Log log)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO logs( log, user_computer, seconds, minutes, hours, day, month, year, level) " +
                    "VALUES( @log, @user_computer, @seconds, @minutes, @hours, @day, @month, @year, @level);", connection))
                {
                    commandSQL.Parameters.AddWithValue("@log", log.log);
                    commandSQL.Parameters.AddWithValue("@user_computer", log.user_computer);
                    commandSQL.Parameters.AddWithValue("@seconds", log.seconds);
                    commandSQL.Parameters.AddWithValue("@minutes", log.minutes);
                    commandSQL.Parameters.AddWithValue("@hours", log.hours);
                    commandSQL.Parameters.AddWithValue("@day", log.day);
                    commandSQL.Parameters.AddWithValue("@month", log.month);
                    commandSQL.Parameters.AddWithValue("@year", log.year);
                    commandSQL.Parameters.AddWithValue("@level", log.level);
                    commandSQL.ExecuteNonQuery();
                    commandSQL.Dispose();
                }
            }
        }
        public List<Log> SelectLogs()
        {
            List<Log> logsMass = new List<Log>();
            using (MySqlConnection connection = new MySqlConnection(connectionstring.ToString()))
            {
                connection.Open();
                using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM logs;", connection))
                {
                    using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                    {
                        while (readerMassive.Read())
                        {
                            Log log = new Log
                            {
                                log = readerMassive.GetString("log"),
                                user_computer = readerMassive.GetString("user_computer"),
                                seconds = readerMassive.GetInt16("seconds"),
                                minutes = readerMassive.GetInt16("minutes"),
                                hours = readerMassive.GetInt16("hours"),
                                day = readerMassive.GetInt16("day"),
                                month = readerMassive.GetInt16("month"),
                                year = readerMassive.GetInt32("year"),
                                level = readerMassive.GetString("level")
                            };
                            logsMass.Add(log);
                        }
                        commandSQL.Dispose();
                    }
                }
            }
            Logger.WriteLog("Select massive logs from database.", LogLevel.Usual);
            return logsMass;
        }
    }
}

using System;
using System.IO;
using System.Text;
using Common.NDatabase;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace Common.Logging
{
    public enum LogLevel { Usual, Warning, Error, Fatal, None }

    public static class Logger
    {
        public static bool stateLogging = true;
        private static string CurrentDirectory = Directory.GetCurrentDirectory();
        private static Semaphore locker = new Semaphore(1,1);
        private static string PathLogsDirectory = "/files/logs/";
        private static string FileName = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
        private static DateTime CurrentFileDate = DateTime.Now;
        private static string Full_Path_File = "";
        private static string UserName = Environment.UserName;
        private static string MachineName = Environment.MachineName;

        private static FileStream FileWriter;
        private static FileInfo FileLogExist;
        /// <summary>
        /// Logging.
        /// </summary>
        /// <param name="logCmd">Log cmd. Is a recorded log.</param>
        /// <param name="level">This is the type of log level</param>
        public static void WriteLog(string logCmd, LogLevel level)
        {
            if (!string.IsNullOrEmpty(logCmd))
            {
                if (logCmd.Length > 2000)
                {
                    logCmd = logCmd.Substring(0, 2000);
                }
            }
            else
            {
                WriteLog("Insert value is null, function WriteLog()", LogLevel.Error);
                return;
            }
            if (stateLogging == true)
            {
                DateTime localDate = DateTime.Now;
                Log loger = new Log
                {
                    log = logCmd,
                    user_computer = UserName + " " + MachineName,
                    seconds = (short)localDate.Second,
                    minutes = (short)localDate.Minute,
                    hours = (short)localDate.Hour,
                    day = (short)localDate.Day,
                    month = (short)localDate.Month,
                    year = localDate.Year,
                    level = SetLevelLog(level)
                };
                CheckExistLogFile(ref localDate);
                Debug.WriteLine(loger.log);
                if (level != LogLevel.None)
                {
                    Write(ref loger);
                }
            }
            else
            {
                Debug.WriteLine(logCmd);
            }
        }
        /// <summary>
        /// Reads logs from database.
        /// </summary>
        /// <returns>The logs database.</returns>
        public static void ReadConsoleLogsDatabase()
        {
            List<Log> logs = Database.log.SelectLogs();
            foreach (Log log in logs)
            {
                Console.WriteLine("Log: " + log.log + ";" + "Data: " + log.year + ":" + log.month + ":" + log.day + ";" +
                "Time: " + log.hours + ":" + log.minutes + ":" + log.seconds + ";" + "Type: " + log.level + ";");
            }
        }
        /// <summary>
        /// Get list of byte (logs) from database.
        /// </summary>
        public static byte[] ReadMassiveLogs()
        {
            List<byte> mass = new List<byte>();
            List<Log> logs = Database.log.SelectLogs();
            foreach (Log log in logs)
            {
                mass.AddRange(Encoding.ASCII.GetBytes("T/D: " + log.hours + ":" + log.minutes + ":" + log.seconds + "__"
                + log.day + ":" + log.month + ":" + log.year
                + "; " + "User_comp.: " + log.user_computer + "; " +
                "Log: " + log.log + "; Level: " + log.level + ";<br>" + "\r\n"));
            }
            return mass.ToArray();
        }
        private static void CheckExistLogFile(ref DateTime localDate)
        {
            Directory.CreateDirectory(CurrentDirectory + PathLogsDirectory);
            Full_Path_File = CurrentDirectory + PathLogsDirectory + FileName;
            if (FileLogExist == null)
            {
                FileLogExist = new FileInfo(Full_Path_File);
            }
            if (!FileLogExist.Exists)
            {
                FileWriter = FileLogExist.Create();
                FileLogExist = new FileInfo(Full_Path_File);
            }
            if (FileWriter == null)
            {
                FileWriter = new FileStream(Full_Path_File, FileMode.Append, FileAccess.Write);
            }
            if (localDate.Day != CurrentFileDate.Day)
            {
                FileWriter.Dispose();
                CurrentFileDate = localDate;
                FileName = CurrentFileDate.Day + "-" + CurrentFileDate.Month + "-" + CurrentFileDate.Year;
                Full_Path_File = CurrentDirectory + PathLogsDirectory + FileName;
                FileWriter = new FileStream(Full_Path_File, FileMode.Append, FileAccess.Write);
            }
        }
        /// <summary>
        /// Write log info to txt file and database.
        /// </summary>
        /// <param name="loger">Loger.</param>
        private static void Write(ref Log loger)
        {
            byte[] array = Encoding.ASCII.GetBytes
            (
                "T/D: " + loger.hours + ":" + loger.minutes + ":" + loger.seconds + "__"
                + loger.day + ":" + loger.month + ":" + loger.year
                + "; " + "User_comp.: " + loger.user_computer + "; " +
                "Log: " + loger.log + "; Level: " + loger.level + ";" + "\r\n"
            );
            locker.WaitOne();
            FileWriter.Write(array, 0, array.Length);
            FileWriter.Flush();
            locker.Release();
            Database.log.AddLogs(ref loger);
        }
        private static string SetLevelLog(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Usual: return "usual";
                case LogLevel.Warning: return "warning";
                case LogLevel.Error: return "error";
                case LogLevel.Fatal: return "fatal";
                case LogLevel.None: return "none";
                default: return "indefinite";
            }
        }
        public static void Dispose()
        {
            if (FileWriter != null)
            {
                FileWriter.Flush();
                FileWriter.Close();
            }
        }
    }
}

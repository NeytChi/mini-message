using System;
using Common.Logging;
using Common.NDatabase;
using System.Diagnostics;
using Common.Chats.Server;
using Newtonsoft.Json.Linq;
using Common.Functional.Mail;
using Common.Functional.UserF;

namespace Common
{
    public class Starter
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and keys functional.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            Config.Initialization();
            Database.Initialization(false);
            MailF.Init();
            ChatServer.Initiation(Config.IP, Config.GetConfigValue("chat_port", JTokenType.Integer));

            if (args.Length != 0)
            {
                switch (args[0])
                {
                    case "-r":
                        Logger.ReadConsoleLogsDatabase();
                        break;
                    case "-c":
                        Database.DropTables();
                        Database.connection.Close();
                        break;
                    case "-v":
                        ChatServer.module.request_view = true;
                        Server server = new Server();
                        server.port = Config.Port;
                        server.ip = Config.IP;
                        server.domen = Config.Domen;
                        UsersController user = new UsersController(Config.Domen);
                        server.InitListenSocket();
                        break;
                    case "-h":
                    case "--h":
                    case "--help":
                    case "-help":
                        Helper();
                        break;
                    default:
                        Console.WriteLine("Turn first parameter for initialize server. You can turned keys: -h or -help - to see instruction of start servers modes.");
                        break;
                }
            }
            else
            {
                Server server = new Server();
                server.port = Config.Port;
                server.ip = Config.IP;
                server.domen = Config.Domen;
                UsersController user = new UsersController(Config.Domen);
                server.InitListenSocket();
            }
        }
        public static void Helper()
        {
            string[] commands = { "-r", "-c", "-v", "-h or -help" };
            string[] description =
            {
                "Start reading logs from server." ,
                "Start the database cleanup mode." ,
                "Start server's listing with request vision mode." ,
                "Helps contains 4 modes of the server that cound be used."
            };
            for (int i = 0; i < commands.Length; i++)
            {
                Console.WriteLine(commands[i] + "\t - " + description[i]);
            }
        }
    }
}
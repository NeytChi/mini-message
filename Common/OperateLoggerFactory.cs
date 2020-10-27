using Serilog;
using Serilog.Events;


namespace mini_message.Common
{
    public class OperateLoggerFactory
    {
        public static bool release = true;
        
        public static ILogger Get()
        {
            if (release)
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            else
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Debug()
                    .WriteTo.ColoredConsole(
                        LogEventLevel.Verbose,
                        "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}")
                    .CreateLogger();
            return Log.Logger;
        }
    }
}
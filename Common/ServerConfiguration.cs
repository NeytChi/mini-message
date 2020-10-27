using System.IO;
using Microsoft.Extensions.Configuration;

namespace mini_message.Common
{
    public static class ServerConfiguration
    {
        private static IConfigurationRoot _configuration;
        
        public static IConfigurationRoot Get()
        {
            if (_configuration == null) {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
            }
            return _configuration;
        }
    }
}
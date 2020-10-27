using Microsoft.Extensions.Configuration;
using mini_message.Common;
using mini_message.Common.Settings;

namespace mini_message
{
    public class UrlsFactory
    {
        public string GetHttp()
        {
            var configurationRoot = ServerConfiguration.Get();
            var hostSettings = configurationRoot.GetSection("HostSettings").Get<HostSettings>();
            return "http://*:" + hostSettings.PortHttp;
        }
    }
}
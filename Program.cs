using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mini_message.Models;
using Serilog;

namespace mini_message
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var context = new Context();
            context.Database.EnsureCreated();
            
            
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseSerilog()
                .UseUrls(new UrlsFactory().GetHttp())
                .Build()
                .Run();
        }
    }
}

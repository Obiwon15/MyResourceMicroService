using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Resourceedge.Common.Logging;

namespace Resourceedge.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseLogging()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    //webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                    //  {
                    //      var env = hostingContext.HostingEnvironment.EnvironmentName;
                    //      config.AddJsonFile($"ocelot.{env}.json");
                    //  });
                });
    }
}


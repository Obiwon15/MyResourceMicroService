using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.API.Services.CronJobServices;
using Resourceedge.Appraisal.Domain.DBContexts;

namespace Resourceedge.Appraisal.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var Dbcontext = scope.ServiceProvider.GetRequiredService(typeof(IDbContext)) as IDbContext;
                var resultAreaService = scope.ServiceProvider.GetRequiredService(typeof(IKeyResultArea)) as IKeyResultArea;
                //InitializerService.Seed(Dbcontext); 
            }
            host.Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });         
    }
}

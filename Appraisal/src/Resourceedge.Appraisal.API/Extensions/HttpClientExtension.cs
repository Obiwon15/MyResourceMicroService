using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Resourceedge.Email.Api.SGridClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Extensions
{
    public static class HttpClientExtension
    {
        public static void RegisterHttpClient(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddTransient<ISGClient, SGClient>(ctx => SGClient.Create(
                Configuration.GetSection("SendGrid:SENDGRID_API_KEY").Value));

            services.AddHttpClient("EmployeeService", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:employee"]);
                config.DefaultRequestHeaders.Clear();
                config.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(800)));

            services.AddHttpClient("discoveryEndpoint", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Authority"]);
                config.DefaultRequestHeaders.Clear();
                config.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient("Auth", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Auth"]);
                config.DefaultRequestHeaders.Clear();
                config.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }
    }
}

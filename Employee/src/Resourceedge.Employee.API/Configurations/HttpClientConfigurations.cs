using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Employee.API.Configurations
{
    public static class HttpClientConfigurations
    {
        public static void AddCustomHttpClients(this IServiceCollection services, string[] Headers)
        {
            IConfiguration Configuration;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                Configuration = serviceProvider.GetService<IConfiguration>();
            }
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(config =>
                {
                    config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders(Headers);
                });
            });

            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", config =>
            {
                //config.Events = new JwtBearerEvents()
                //{
                //    OnMessageReceived = context =>
                //    {
                //        var aa = context.Request.Headers["HeaderAuthorization"];
                //        if (context.Request.Query.ContainsKey("access_token"))
                //        {
                //            context.Token = context.Request.Query["access_token"];
                //        }
                //        else
                //        {
                //            context.Token = context.Request.Headers["HeaderAuthorization"][0];
                //        }
                //        return Task.CompletedTask;
                //    }
                //};
                //config.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                //{
                //    ValidIssuer = Configuration["Services:Authority"],
                //    ValidAudiences = new string[] { "Employee", "Appraisal" }
                //};
                config.Authority = Configuration["Services:Authority"];
                config.Audience = "Employee";
                config.TokenValidationParameters.ValidAudiences = new string[] { "Employee" };
                config.RequireHttpsMetadata = false;
            });

            services.AddHttpClient("OldEdge", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:oldResourceedge"]);
            });
        }

    }
}

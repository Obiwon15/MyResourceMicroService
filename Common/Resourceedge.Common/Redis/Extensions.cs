using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Redis
{
    public static class Extensions
    {
        private static readonly string SectionName = "redis";

        public static IServiceCollection AddRedis(this IServiceCollection services)
        {
            IConfiguration configuration;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }

            services.Configure<RedisOptions>(configuration.GetSection(SectionName));
            var options = configuration.GetOptions<RedisOptions>(SectionName);
            services.AddDistributedRedisCache(o =>
            {
                o.Configuration = options.ConnectionString;
                o.InstanceName = options.Instance;
            });

            return services;
        }
    }
}

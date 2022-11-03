using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Resourceedge.Common.Mongo.Interfaces;
using Resourceedge.Common.Types.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Mongo.Extensions
{
    public static class Extensions
    {
        private static readonly string SectionName = "mongo";
        public static void AddMongo(this IServiceCollection services)
        {
            IConfiguration configuration;
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();

            }
            var aa = configuration.GetOptions<MongoDbOptions>(SectionName);
            services.AddSingleton<IMongoDbOptions>(sp => configuration.GetOptions<MongoDbOptions>(SectionName));
            services.AddSingleton(cfg => new MongoClient(cfg.GetService<IMongoDbOptions>().ConnectionString));
            services.AddSingleton(cfg =>
            {
                var options = cfg.GetRequiredService<IMongoDbOptions>();
                var client = cfg.GetRequiredService<MongoClient>();
                return client.GetDatabase(options.DatabaseName);
            });
            services.AddSingleton<IMongoDbInitializer, MongoDbInitializer>();
            services.AddSingleton<IMongoDbSeeder, MongoDbSeeder>();
        }

        public static void AddMongoRepository<TEntity>(this IServiceCollection services, string collectionName) where TEntity : Entity
        {
            IConfiguration configuration;
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }
            services.AddTransient<IMongoRepository<TEntity>>
                (cfg => new MongoRepository<TEntity>(cfg.GetService<IMongoDatabase>(), collectionName,
                cfg.GetService<ILogger<MongoRepository<TEntity>>>()));
        }
    }
}

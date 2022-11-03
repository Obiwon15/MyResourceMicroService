using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Resourceedge.Common.Mongo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.Mongo
{
    public  class MongoDbSeeder : IMongoDbSeeder
    {
        protected readonly IMongoDatabase Database;
        private readonly IConfiguration _configuration;

        public MongoDbSeeder(IMongoDatabase database)
        {
            Database = database;
        }

        //public MongoDbSeeder(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //    //var aa = configuration.
        //}

        public async Task SeedAsync()
        {

            await CustomSeedAsync();
        }

        protected virtual async Task CustomSeedAsync()
        {
            var cursor = await Database.ListCollectionsAsync();
            var collections = await cursor.ToListAsync();
            if (collections.Any())
            {
                return;
            }

            await Task.CompletedTask;
        }
    }
}

using Resourceedge.Common.Mongo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Mongo
{
    public class MongoDbOptions : IMongoDbOptions
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public bool Seed { get; set; }
    }
}

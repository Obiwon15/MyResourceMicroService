using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Mongo.Interfaces
{
    public interface IMongoDbOptions
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        public bool Seed { get; set; }
    }
}

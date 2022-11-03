using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.Mongo.Interfaces
{
    public interface IMongoDbSeeder
    {
        Task SeedAsync();
    }
}

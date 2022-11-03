using MongoDB.Driver;
using Resourceedge.Common.Mongo.Interfaces;
using Resourceedge.Common.Types;
using Resourceedge.Common.Mongo.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Resourceedge.Common.Types.Entities;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Logging;

namespace Resourceedge.Common.Mongo
{
    public class MongoRepository<TEntity> : IMongoRepository<TEntity> where TEntity : Entity
    {
        private readonly ILogger<MongoRepository<TEntity>> _logger;

        protected IMongoCollection<TEntity> _collection { get; }
        public IMongoCollection<TEntity> Collection { get; }

        public MongoRepository(IMongoDatabase database, string collectionName, ILogger<MongoRepository<TEntity>> logger)
        {
            if (database is null)
            {
                throw new ArgumentNullException(nameof(database));
            }
            if (string.IsNullOrEmpty(collectionName) || string.IsNullOrWhiteSpace(collectionName))
            {
                throw new ArgumentNullException(nameof(collectionName));
            }
            _collection = database.GetCollection<TEntity>(collectionName);
            _logger = logger;
            Collection = _collection;
        }

        public async Task<TEntity> GetAsync(string id)
           => await GetAsync(e => e.Id == id);

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
            => await _collection.Find(predicate).SingleOrDefaultAsync();

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
            => await _collection.Find(predicate).ToListAsync();

        public async Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate,
                TQuery query) where TQuery : PagedQueryBase
            => await _collection.AsQueryable().Where(predicate).PaginateAsync(query);

        public async Task AddAsync(TEntity entity)
            => await _collection.InsertOneAsync(entity);

        public async Task UpdateAsync(TEntity entity)
            => await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);

        public async Task DeleteAsync(string id)
            => await _collection.DeleteOneAsync(e => e.Id == id);

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
            => await _collection.Find(predicate).AnyAsync();
    }
}

using MongoDB.Driver;
using Resourceedge.Common.Types;
using Resourceedge.Common.Types.Entities;
using Resourceedge.Common.Types.Entities.Interfaces;
using Resourceedge.Common.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.Mongo.Interfaces
{
    public interface IMongoRepository<TEntity> where TEntity : Entity
    {
        public IMongoCollection<TEntity> Collection { get; }
        Task<TEntity> GetAsync(string id);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate,
               TQuery query) where TQuery : PagedQueryBase;
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    }
}

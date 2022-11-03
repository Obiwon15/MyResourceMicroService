using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Resourceedge.Common.Types;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.Mongo.Extensions
{
    public static class Pagination
    {
        public static async Task<PagedResult<T>> PaginateAsync<T>(this IMongoQueryable<T> collection, PagedQueryBase query)
            => await collection.PaginateAsync(query.Page, query.Results);

        public static async Task<PagedResult<T>> PaginateAsync<T>(this IMongoQueryable<T> collection, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var isEmpty = await collection.AnyAsync() == false;
            if (isEmpty)
            {
                return PagedResult<T>.Empty;
            }

            var totalResults = await collection.CountAsync();
            var totalPages = (int)Math.Ceiling((decimal)totalResults / pageSize);
            var data = await collection.Limit(pageNumber, pageSize).ToListAsync();
            return PagedResult<T>.Create(data, pageNumber, pageSize, totalPages, totalResults);
        }

        public static IMongoQueryable<T> Limit<T>(this IMongoQueryable<T> collection, PagedQueryBase query)
            => collection.Limit(query.Page, query.Results);

        public static IMongoQueryable<T> Limit<T>(this IMongoQueryable<T> collection,
           int page = 1, int pageSize = 10)
        {
            if (page <= 0)
            {
                page = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var skip = (page - 1) * pageSize;
            var data = collection.Skip(skip)
                .Take(pageSize);

            return data;
        }

        public static async Task<PagedResult<T>> PaginateAsync<T>(this IMongoCollection<T> collection, IMongoCollection<T> aa, int pageNumber = 1, int pageSize = 10)
        {
            var queryableCollection = collection.AsQueryable<T>();
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var isEmpty = await queryableCollection.AnyAsync() == false;
            if (isEmpty)
            {
                return PagedResult<T>.Empty;
            }

            var totalResults = await collection.EstimatedDocumentCountAsync();
            var totalPages = (int)Math.Ceiling((decimal)totalResults / pageSize);
            var data = await queryableCollection.Limit(pageNumber, pageSize).ToListAsync();
            return PagedResult<T>.Create(data, pageNumber, pageSize, totalPages, totalResults);
        }
    }
}

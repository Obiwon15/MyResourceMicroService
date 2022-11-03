using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Resourceedge.Common.Types
{
    public class PagedResult<T> : PagedResultBase
    {
        public IEnumerable<T> Items { get; }
        public bool IsEmpty => Items == null || !Items.Any();
        public bool IsNotEmpty => !IsEmpty;
        
        protected PagedResult()
        {
            Items = Enumerable.Empty<T>();
        }

        [JsonConstructor]
        protected PagedResult(IEnumerable<T> items,
            int currentPage, int pageSize,
            int totalPages, long totalResults) : 
            base(currentPage, pageSize, totalPages, totalResults)
        {
            Items = items;
        }

        public static PagedResult<T> Create(IEnumerable<T> items, int currentPage,
            int pageSize, int totalPages, long totalResults)
            => new PagedResult<T>(items, currentPage, pageSize, totalPages, totalResults);

        public static PagedResult<T> From(PagedResultBase result, IEnumerable<T> items)
            => new PagedResult<T>(items, result.CurrentPage, result.PageSize,
                result.TotalPages, result.TotalResults);

        public static PagedResult<T> Empty => new PagedResult<T>();
    }

}

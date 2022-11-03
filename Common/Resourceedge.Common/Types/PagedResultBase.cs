using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Types
{
    public abstract class PagedResultBase
    {
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get;  }
        public bool HasPrevious { get;  }
        public bool HasNext { get;  }
        public long TotalResults { get; }

        protected PagedResultBase()
        {

        }

        public PagedResultBase(int currentPage, int pageSize, int totalPages, long totalResults )
        {
            CurrentPage = CurrentPage > totalPages ? totalPages : currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            TotalResults = totalResults;
            HasPrevious = (CurrentPage > 1);
            HasNext = (CurrentPage < TotalPages);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Util
{
    public class PaginationResourceParameter
    {
        const int maxPageSize = 20;
        public string SearchQuery { get; set; }
        public string Company { get; set; }
        public string Score { get; set; }
        public string Status { get; set; }
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 18;
        public int PageSize
        {
            get => _pageSize;

            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public string OrderBy { get; set; }
        public string Fields { get; set; }
    }

    public class PaginationResourceResponse
    {
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } 
        public bool HasNext { get; set; }
        public int Next { get; set; }
        public int prev { get; set; }

        public PaginationResourceResponse(int count, int currentPage, int pageSize)
        {
            CurrentPage = currentPage;
            TotalCount = count;
            prev = 0;
            var res = Math.Round(((decimal)count / ((decimal)pageSize * currentPage)),2);
            HasNext = res > currentPage;

            if(currentPage > 1)
            {
                prev = currentPage - 1;
            }

            if (HasNext)
            {
                Next = currentPage + 1;
            }
        }
    }

}

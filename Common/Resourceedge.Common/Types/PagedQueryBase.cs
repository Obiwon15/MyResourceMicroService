using Resourceedge.Common.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Types
{
    public abstract class PagedQueryBase : IPagedQuery
    {
        public int Page { get; set; }
        public int Results { get; set; }
        public string OrderBy { get; set; }
        public string SortOrder { get; set; }
    }
}

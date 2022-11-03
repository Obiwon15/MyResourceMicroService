using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Dtos
{
    public class DirectReportDto
    {
        public PagedList<AppraisalForApprovalViewDto> DirectReport { get; set; }
        public PaginationResourceResponse Pagination { get; set; }
    }

    public class AppraisalReport
    {
        public PagedList<AppraisalsForReviewDto> EmployeeReport { get; set; }
        public PaginationResourceResponse Pagination { get; set; }
    }
}

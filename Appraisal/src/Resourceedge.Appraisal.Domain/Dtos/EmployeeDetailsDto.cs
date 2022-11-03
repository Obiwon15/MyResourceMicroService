using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Common.Archive;
using Resourceedge.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Dtos
{
    public class EmployeeDetailsDto
    {
        public PagedList<emloyeeKraApprovalDto> EmployeesWithDetails { get; set; }
        public PaginationResourceResponse Pagination { get; set; }
    }

    public class emloyeeKraApprovalDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string JobRole { get; set; }
        public string SubGroup { get; set; }
        public string Status { get; set; }
    }

}

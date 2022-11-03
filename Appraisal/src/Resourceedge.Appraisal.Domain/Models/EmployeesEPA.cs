using MongoDB.Bson.Serialization.Attributes;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Common.Archive;
using Resourceedge.Common.Models;
using Resourceedge.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Models
{
    [BsonIgnoreExtraElements]
    public class EmployeesEPA
    {
        public int _id { get; set; }
        public bool Approved { get; set; }
        public string Status { get; set; }
        public TeamLead TeamLead { get; set; }
        public BasicEmployee EmployeeDetail { get; set; }
    }


    public class EmployeesEPAWithPagination
    {
        public List<EmployeesEPA> EmployeesEPA { get; set; }
        public PaginationResourceResponse Pagination { get; set; }
    }
}

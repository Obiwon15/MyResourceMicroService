using MongoDB.Bson.Serialization.Attributes;
using Resourceedge.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Models
{
    [BsonIgnoreExtraElements]
    public class AppraisalsForReviewDto
    {
        public int EmployeeId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public String Company { get; set; }
        public double EmployeeResult { get; set; }
        public double AppraiseeResult { get; set; }
        public double FinalResult { get; set; }
        public string Status { get; set; }
    }
}

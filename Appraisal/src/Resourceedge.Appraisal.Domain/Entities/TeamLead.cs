using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Entities
{
    public class TeamLead
    {
        public ObjectId Id { get; set; }
        public int EmployeeId { get; set; }
        public int TeamLeadId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int Year { get; set; } = DateTime.Now.Year;

    }
}

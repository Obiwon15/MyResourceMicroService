using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Resourceedge.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Entities
{

    [BsonIgnoreExtraElements]
    public class KeyResultArea
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public decimal Weight { get; set; }
        [BsonIgnoreIfDefault]
        public NameEmail HodDetails { get; set; } = new NameEmail();
        [BsonIgnoreIfDefault]
        public NameEmail AppraiserDetails { get; set; } = new NameEmail();
        public bool? Approved { get; set; }
        public int Year { get; set; } = DateTime.Now.Year;
        public bool IsActive { get; set; }
        public IEnumerable<KeyOutcome> keyOutcomes { get; set; } = new List<KeyOutcome>();
        public ApprovalStatus Status { get; set; } = new ApprovalStatus();
        public BsonDateTime CreatedAt { get; set; }
        public BsonDateTime UpdatedAt { get; set; }


    }

    [BsonIgnoreExtraElements]
    public class NameEmailForResultArea : NameEmail
    {
        //public string Type { get; set; }
    }

    public class KeyOutcome
    {
        public string Title { get; set; }
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string Question { get; set; }
        public string TimeLimit { get; set; }
        [BsonIgnoreIfNull]
        public ApprovalStatus Status { get; set; }
    }
    public class ApprovalStatus
    {
        [BsonIgnoreIfNull]
        public bool? TeamLead { get; set; }
        [BsonIgnoreIfNull]
        public string Reason { get; set; }
        public bool? Employee { get; set; } = null;
        public bool? IsAccepted { get; set; } = null;
        [BsonIgnoreIfNull]
        public bool? Hod { get; set; } = null;
        [BsonIgnoreIfNull]
        public bool? Appraiser { get; set; } = null;
    }
}

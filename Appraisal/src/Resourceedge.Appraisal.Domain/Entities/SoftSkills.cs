using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Resourceedge.Appraisal.Domain.Entities
{
    public class SoftSkills
    {
        public SoftSkills()
        {
            keyOutcomes = new List<KeyOutcome>();
            Year = DateTime.Now.Year;
        }

        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public decimal Weight { get; set; }
        public bool? Approved { get; set; }
        public int Year { get; set; } 
        public bool IsActive { get; set; }
        public ICollection<KeyOutcome> keyOutcomes { get; set; } 
        public BsonDateTime CreatedAt { get; set; }
        public BsonDateTime UpdatedAt { get; set; }
    }
}

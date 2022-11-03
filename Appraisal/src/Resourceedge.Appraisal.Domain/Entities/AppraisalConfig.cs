using MongoDB.Bson;
using Resourceedge.Appraisal.Domain.Dtos;
using System;
using System.Collections.Generic;

namespace Resourceedge.Appraisal.Domain.Entities
{
    public class AppraisalConfig
    {
        public ObjectId Id { get; set; }
        public ReviewType ReviewType  { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PeriodInReview Period { get; set; }
        public Duration Duration { get; set; }
        public bool? isActive { get; set; }
        public bool? IsArchived { get; set; }
        public bool? Completed { get; set; }
        public DateTime CreateAt { get; set; }
        public int TotalCycle { get; set; }
        public int Year { get; set; }
        public Participants Participants { get; set; }
        public List<AppraisalCycle> Cycles{ get; set; }

    }

    public class AppraisalCycle
    {
        public AppraisalCycle()
        {
            Completed = (this.isActive.HasValue && this.isActive.Value) ? false : (this.isActive.HasValue && !this.isActive.Value) ? true : (bool?) null;
        }
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public DateTime StartDate { get; set; } 
        public DateTime StopDate { get; set; }
        public bool? isActive { get; set; }
        public string Name { get; set; }
        public bool? Completed { get; set; }
        public bool? IsClosed { get; set; }
        public bool? isArchived { get; set; }

    }
}

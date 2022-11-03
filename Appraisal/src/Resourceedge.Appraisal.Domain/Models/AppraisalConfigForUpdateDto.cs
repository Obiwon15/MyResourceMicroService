using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Models
{
    public class AppraisalConfigForUpdateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public PeriodInReview Period { get; set; }
        public Duration Duration { get; set; }
        public Participants Participants { get; set; }
        public ReviewTypeDto ReviewType { get; set; }

    }

}

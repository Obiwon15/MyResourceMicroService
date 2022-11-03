using MongoDB.Bson;
using Resourceedge.Appraisal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Dtos
{
    public class ConfigAppraisalViewDto
    {
        public ObjectId Id { get; set; }
        public ReviewTypeDto ReviewType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PeriodInReview Period { get; set; }
        public Duration Duration { get; set; }
        public bool? isActive { get; set; }
        public bool? IsArchived { get; set; }
        public bool? Completed { get; set; }
        public int Year { get; set; }
        public Participants Participants { get; set; }
        public string Status { get; set; }
    }

    public class ConfigAppraisalWithParticipantDetail
    {
        public ConfigAppraisalViewDto configAppraisal { get; set; }
        public ParticipantsWithDetail participants { get; set; }        
    }

    public class ReviewTypeDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
   
}

using MongoDB.Bson;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Dtos
{
    public  class AppraisalPerformanceDto
    {
        public int myId { get; set; }
        public string whoami { get; set; }
        public string KeyResultAreaId { get; set; }
        public ICollection<AppraisalKeyOutcomeDtoString> KeyOutcomeScore { get; set; }
        public FeedBack AppraiseeFeedBack { get; set; }
    }

    public class AppraisalPerforanceForCreationDto
    {
        public int myId { get; set; }
        public string whoami { get; set; }
        public ObjectId KeyResultAreaId { get; set; }
        public ICollection<AppraisalKeyOutcomeDto> KeyOutcomeScore { get; set; }
        public FeedBack AppraiseeFeedBack { get; set; }
    }

    public class AppraisalPerformanceParam
    {
        public int empId { get; set; }
        public string reviewId { get; set; }
    }
}

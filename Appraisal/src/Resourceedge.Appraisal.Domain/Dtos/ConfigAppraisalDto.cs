using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Dtos
{
    public class ConfigAppraisalDto
    {
        public ReviewTypeDto ReviewType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PeriodInReview Period { get; set; }
        public Duration Duration { get; set; }
        public Participants Participants { get; set; }
    }

    public class PeriodInReview
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class Duration
    {
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
    }

    public class Participants
    {
        public IEnumerable<string> Include { get; set; }
        public IEnumerable<string> Exclude { get; set; }
    }

    public class ParticipantsWithDetail
    {
        public IEnumerable<NameEmailWithFullName> Include { get; set; }
        public IEnumerable<NameEmailWithFullName> Exclude { get; set; }
    }
}

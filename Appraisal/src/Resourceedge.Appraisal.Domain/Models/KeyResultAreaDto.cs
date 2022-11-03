using MongoDB.Bson;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Common.Models;
using System.Collections.Generic;

namespace Resourceedge.Appraisal.Domain.Models
{
    public class KeyResultAreaDtoForCreation
    {
        public string Name { get; set; }
        public int myId { get; set; }
        public decimal Weight { get; set; }
        public NameEmail HeadOfDepartment { get; set; }
        public NameEmail Appraiser { get; set; }
        public ICollection<KeyOutcomeForCreationDto> keyOutcomes { get; set; } = new List<KeyOutcomeForCreationDto>();
        public ApprovalStatus Status { get; set; } = new ApprovalStatus();
    }

    public class KeyOutcomeForCreationDto
    {
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string Question { get; set; }
        public string TimeLimit { get; set; }
    }

    public class KeyResultAreaForViewDto : KeyResultAreaDtoForCreation
    {
        public ObjectId Id { get; set; }
        public bool? Approved { get; set; }
        public int Year { get; set; }

    }





    public class AppraisalKeyResultAreaForViewDto
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public int EmployeeId { get; set; }
        public decimal Weight { get; set; }
        public NameEmail HeadOfDepartment { get; set; } = new NameEmail();
        public NameEmail Appraiser { get; set; } = new NameEmail();
        public bool? Approved { get; set; }
        public string whoami { get; set; }
        public IEnumerable<KeyOutcome> keyOutcomes { get; set; } = new List<KeyOutcome>();
        public ApprovalStatus Status { get; set; } = new ApprovalStatus();
        public AppraisalCalculationByKRA EmployeeCalculation { get; set; } = new AppraisalCalculationByKRA();
        public AppraisalCalculationByKRA FinalCalculation { get; set; } = new AppraisalCalculationByKRA();
    }

    public  class  KeyResultAreaSuperviorCliams
    {
        public NameEmail Appraiser { get; set; }
        public NameEmail Hod { get; set; }
    }
}

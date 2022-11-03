using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Models
{
    public class ChangeSupervisorDto
    {
        public ObjectId KeyResultAreaId { get; set; }
        public int OldSupervisorId { get; set; }
        public int NewSupervisorId { get; set; }
        public string WhoToUpdate { get; set; }

    }

    public class ChangeSupervisorDtoForView
    {
        public string keyresultareaid { get; set; }
        public int oldsupervisorid { get; set; }
        public int newsupervisorid { get; set; }
        public string whotoupdate { get; set; }
    }

}

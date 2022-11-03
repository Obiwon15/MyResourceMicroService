using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Employee.Domain.Models
{
    class EmployeeHistories
    {
        public string HighestDegree  { get; set; }
        
    }

    public class WorkHIstroy
    {
        public string NameofOrganisation { get; set; }
        public string Industry { get; set; }
        public string SizeofOrganisation { get; set; }
        public string JobRole { get; set; }
        public int YearsofExperience { get; set; }
    }
}

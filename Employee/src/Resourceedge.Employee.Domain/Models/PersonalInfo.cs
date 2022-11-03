using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Employee.Domain.Models
{
    public class PersonalInfo
    {
        public DateTime? DateofBirth { get; set; }
        public string Gender { get; set; }
        public string MartialStatus { get; set; }
        public string Nationality { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}

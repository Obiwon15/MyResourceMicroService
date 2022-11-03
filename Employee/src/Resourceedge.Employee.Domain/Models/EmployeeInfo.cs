using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Employee.Domain.Models
{
    public class EmployeeInfo
    {
        public int EmployeeId { get; set; }
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string JobRole { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}

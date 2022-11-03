using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Employee.Domain.Models
{
    public class EmployeeBioDataDto
    {
        public EmployeeBioDataDto()
        {
            PersonalInformation = new PersonalInfo();
            HealthInformation = new HealthRecord();
            Work = new WorkHIstroy(); 
            NextofKin = new NextofKin();
            Spouse = new Spouse();
            CreatedAt = BsonDateTime.Create(DateTime.Now);
        }
        public int EmployeeId { get; set; }
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string JobRole { get; set; }
        public string OfficalEmail { get; set; }
        public string WorkPhone { get; set; }
        public string Company { get; set; }
        // public EmployeeInfo EmploymentInformation { get; set; } = new EmployeeInfo();
        public PersonalInfo PersonalInformation { get; set; } 
        public HealthRecord HealthInformation { get; set; } 
        public string EducationLevel { get; set; }
        public WorkHIstroy Work { get; set; } 
        public NextofKin NextofKin { get; set; } 
        public Spouse Spouse { get; set; } 
        public BsonDateTime CreatedAt { get; set; } 

    }

    public class EmployeeCreateDto
    {
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string JobRole { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string Level { get; set; }
        public string EmploymentType { get; set; }
        public string ResumptionDate { get; set; }
    }
}

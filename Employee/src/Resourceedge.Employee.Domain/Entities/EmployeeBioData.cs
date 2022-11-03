using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Resourceedge.Employee.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Employee.Domain.Entities
{
    public  class EmployeeBioData
    {
        public EmployeeBioData()
        {
            //EmploymentInformation = new EmployeeInfo();
            PersonalInformation = new PersonalInfo();
            HealthInformation = new HealthRecord();
            Work = new WorkHIstroy();
            NextofKin = new NextofKin();
            Spouse = new Spouse();
            CreatedAt = new BsonDateTime(DateTime.Now);
        }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public ObjectId Id { get; set; }
        public int EmployeeId { get; set; }
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
        //public EmployeeInfo EmploymentInformation { get; set; } 
        public PersonalInfo PersonalInformation { get; set; } 
        public HealthRecord HealthInformation { get; set; } 
        public string EducationLevel { get; set; }
        public WorkHIstroy Work { get; set; }
        public NextofKin NextofKin { get; set; } 
        public Spouse Spouse { get; set; }
        public BsonDateTime CreatedAt { get; set; } 
        public BsonDateTime? UpdatedAt { get; set; }
    }
}

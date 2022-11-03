using MongoDB.Bson;
using Resourceedge.Common.Types.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Employee.Domain.Entities
{
    public class Person : Entity
    {
        public Person()
        {
           
        }

        public Person(string id,  string firstname, string lastname, string userId) : base(id, userId)
        {
            FirstName = firstname;
            LastName = lastname;
            Id = id;
        }

        public string FirstName { get; }
        public string LastName { get; }
        
    }
}

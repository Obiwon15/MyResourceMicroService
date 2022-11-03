using Newtonsoft.Json;
using Resourceedge.Common.Messages;
using System;

namespace Resourceedge.Employee.Application.Events
{
    public class PersonCreated : IEvent
    {
        //public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [JsonConstructor]
        public PersonCreated(string firstName, string lastName)
        {
            FirstName = firstName;
            //Id = id;
            LastName = lastName;
        }
    }
}

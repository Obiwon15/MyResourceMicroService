using Newtonsoft.Json;
using Resourceedge.Common.Messages;
using System;

namespace Resourceedge.Operations.Api.Messages.Employee.Events
{
    [MessageNamespace("employee")]
    public class PersonCreated : IEvent
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [JsonConstructor]
        public PersonCreated(Guid id, string firstName, string lastName)
        {
            FirstName = firstName;
            Id = id;
            LastName = lastName;
        }
    }
}

using Newtonsoft.Json;
using Resourceedge.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.ApiGateway.Messages.Commands.Persons
{
    [MessageNamespace("employee")]
    public class CreatePerson : ICommand
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string Id { get; }

        [JsonConstructor]
        public CreatePerson(string id, string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            Id = id;
        }
    }
}

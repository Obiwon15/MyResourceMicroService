using Newtonsoft.Json;
using Resourceedge.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Employee.Application.Commands
{
    public class CreatePerson : ICommand
    {
        public string FirstName { get;  }
        public string LastName { get; }
        public string Id { get;  }

        [JsonConstructor]
        public CreatePerson(string id, string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            Id = id;
        }
    }
}

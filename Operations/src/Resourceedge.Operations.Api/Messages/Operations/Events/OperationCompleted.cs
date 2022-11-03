using Newtonsoft.Json;
using Resourceedge.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Operations.Api.Messages.Operations.Events
{
    public class OperationCompleted : IEvent
    {
        public Guid Id { get; }
        public string UserId { get; }
        public string Name { get; }
        public string Resource { get; }

        [JsonConstructor]
        public OperationCompleted(Guid id,
            string userId, string name, string resource)
        {
            Id = id;
            UserId = userId;
            Name = name;
            Resource = resource;
        }
    }
}

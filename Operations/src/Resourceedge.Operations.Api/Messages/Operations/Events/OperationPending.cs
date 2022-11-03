using Newtonsoft.Json;
using Resourceedge.Common.Messages;
using System;

namespace Resourceedge.Operations.Api.Messages.Operations.Events
{
    public class OperationPending : IEvent
    {
        public Guid Id { get; }
        public string UserId { get; }
        public string Name { get; }
        public string Resource { get; }

        [JsonConstructor]
        public OperationPending(Guid id,
            string userId, string name, string resource)
        {
            Id = id;
            UserId = userId;
            Name = name;
            Resource = resource;
        }
    }
   
}

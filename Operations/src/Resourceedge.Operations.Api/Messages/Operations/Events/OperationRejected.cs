using Newtonsoft.Json;
using Resourceedge.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Operations.Api.Messages.Operations.Events
{
    public class OperationRejected : IEvent
    {
        public Guid Id { get; }
        public string UserId { get; }
        public string Name { get; }
        public string Resource { get; }
        public string Code { get; }
        public string Message { get; }

        [JsonConstructor]
        public OperationRejected(Guid id,
            string userId, string name, string resource,
            string code, string message)
        {
            Id = id;
            UserId = userId;
            Name = name;
            Resource = resource;
            Code = code;
            Message = message;
        }
    }
}

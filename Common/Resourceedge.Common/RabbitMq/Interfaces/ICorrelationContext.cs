using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.interfaces.RabbitMq
{
    public interface ICorrelationContext
    {
        Guid Id { get; }
        string UserId { get; }
        string ResourceId { get; }
        string TraceId { get; }
        string SpanContext { get; }
        string ConnectionId { get; }
        string Name { get; }
        string Origin { get; }
        string Resource { get; }
        string Culture { get; }
        DateTime CreatedAt { get; }
        int Retries { get; set; }
    }
}

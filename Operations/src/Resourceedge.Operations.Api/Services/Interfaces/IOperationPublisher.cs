using Resourceedge.Common.interfaces.RabbitMq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Operations.Api.Services.Interfaces
{
    public interface IOperationPublisher
    {
        Task PendingAsync(ICorrelationContext context);
        Task CompleteAsync(ICorrelationContext context);
        Task RejectAsync(ICorrelationContext context, string code, string message);
    }
}

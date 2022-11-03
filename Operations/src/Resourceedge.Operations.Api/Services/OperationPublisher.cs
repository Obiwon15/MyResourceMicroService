using Resourceedge.Common.interfaces.RabbitMq;
using Resourceedge.Operations.Api.Messages.Operations.Events;
using Resourceedge.Operations.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Operations.Api.Services
{
    public class OperationPublisher : IOperationPublisher
    {
        private readonly IBusPublisher _busPublisher;

        public OperationPublisher(IBusPublisher busPublisher)
        {
            _busPublisher = busPublisher;
        }

        public async Task PendingAsync(ICorrelationContext context)
            => await _busPublisher.PublishAsync(new OperationPending(context.Id,
                context.UserId, context.Name, context.Resource), context);

        public async Task CompleteAsync(ICorrelationContext context)
            => await _busPublisher.PublishAsync(new OperationCompleted(context.Id,
                context.UserId, context.Name, context.Resource), context);

        public async Task RejectAsync(ICorrelationContext context, string code, string message)
            => await _busPublisher.PublishAsync(new OperationRejected(context.Id,
                context.UserId, context.Name, context.Resource, code, message), context);
    }
}

using Resourceedge.Common.Handlers.Interfaces;
using Resourceedge.Common.interfaces.RabbitMq;
using Resourceedge.Common.Messages;
using Resourceedge.Operations.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Operations.Api.Handlers
{
    /// <summary>
    /// Generic implementation for handlers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericCommandHandler<T> : ICommandHandler<T> where T : class, ICommand
    {
        private readonly IOperationPublisher _operationPublisher;
        private readonly IOperationsStorage _operationsStorage;

        public GenericCommandHandler(
            //ISagaCoordinator sagaCoordinator,
           IOperationPublisher operationPublisher,
           IOperationsStorage operationsStorage)
        {
            //_sagaCoordinator = sagaCoordinator;
            _operationPublisher = operationPublisher;
            _operationsStorage = operationsStorage;
        }
        public async Task HandleAsync(T command, ICorrelationContext context)
        {
            //if (!command.BelongsToSaga())
            //{
            //    return;
            //}

            //var sagaContext = SagaContext.FromCorrelationContext(context);
            //await _sagaCoordinator.ProcessAsync(command, sagaContext);
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resourceedge.Common.Dispatchers.Interfaces;
using Resourceedge.Common.Handlers.Interfaces;
using Resourceedge.Common.Messages;
using Resourceedge.Common.RabbitMq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceFactory _factory;

        public CommandDispatcher(IServiceFactory factory)
        {
            _factory = factory;
        }

        public async Task SendAsync<T>(T command) where T : ICommand
        {
            var Handler = _factory.GetServices<ICommandHandler<T>>();
            await Handler.HandleAsync(command, CorrelationContext.Empty);
        }
    }
}

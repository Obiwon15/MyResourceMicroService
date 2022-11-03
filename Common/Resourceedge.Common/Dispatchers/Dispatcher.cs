using Resourceedge.Common.Dispatchers.Interfaces;
using Resourceedge.Common.Messages;
using Resourceedge.Common.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.Dispatchers
{
    public class Dispatcher : IDispatcher
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        public Dispatcher(ICommandDispatcher commandDispatcher,
            IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        public async Task SendAsync<TCommand>(TCommand command) where TCommand : ICommand
            => await _commandDispatcher.SendAsync(command);

        public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
            => await _queryDispatcher.QueryAsync<TResult>(query);
    }
}

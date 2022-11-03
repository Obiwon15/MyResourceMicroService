using Resourceedge.Common.Dispatchers.Interfaces;
using Resourceedge.Common.Handlers.Interfaces;
using Resourceedge.Common.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceFactory _factory;

        public QueryDispatcher(IServiceFactory factory)
        {
            _factory = factory;
        }

        public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
        {
            var handlerType = typeof(IQueryHandler<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            dynamic handler = _factory.GetServices(handlerType);

            return await handler.HandleAsync((dynamic)query);
        }
    }
}

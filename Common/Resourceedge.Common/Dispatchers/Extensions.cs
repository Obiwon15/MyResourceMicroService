using Microsoft.Extensions.DependencyInjection;
using Resourceedge.Common.Dispatchers.Interfaces;

namespace Resourceedge.Common.Dispatchers
{
    public static class Extensions
    {
        public static void AddDispatchers(this IServiceCollection services)
        {
            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
            services.AddSingleton<IDispatcher, Dispatcher>();
            services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        }
    }
}

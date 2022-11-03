using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Common
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider provider;

        public ServiceFactory(IServiceProvider _provider)
        {
            provider = _provider;
        }

        public object GetServices(Type service)
        {
            var result = provider.GetService(service);
            return result;
        }

        public TEntity GetServices<TEntity>(params Type[] services) where TEntity : class
        {
            var service = provider.GetService<TEntity>();
            return (TEntity)service;
        }

        public TEntity GetServices<TEntity>(Func<IServiceProvider, TEntity> func) where TEntity : class
        {
            return func(provider);
        }
    }
}

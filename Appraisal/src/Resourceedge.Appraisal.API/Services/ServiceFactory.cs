using Microsoft.Extensions.DependencyInjection;
using Resourceedge.Appraisal.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Services
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider provider;

        public ServiceFactory(IServiceProvider _provider)
        {
            provider = _provider;
        }

        public TEntity GetServices<TEntity>(params Type[] services) where TEntity : class
        {
            //var res = provider.GetRequiredService(services[0]);
            var service = provider.GetService<TEntity>();
            return (TEntity) service;
        }
    }
}

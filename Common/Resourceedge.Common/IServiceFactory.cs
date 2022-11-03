using Resourceedge.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Common
{
    public interface IServiceFactory 
    {
        TEntity GetServices<TEntity>(params Type[] services) where TEntity : class;
        object GetServices(Type service);
        TEntity GetServices<TEntity>(Func<IServiceProvider, TEntity> func) where TEntity : class;
    }
}

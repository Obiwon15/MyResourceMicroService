using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common
{
    /// <summary>
    /// The services found here are default services and shoul be implemented by all services
    /// </summary>
    public static class DefaultServices
    {
        public static void AddDefaultEdgeServices(this IServiceCollection services)
        {
            services.AddTransient<IServiceFactory, ServiceFactory>();
            services.AddTransient<IServiceId, ServiceId>();
        }
    }
}

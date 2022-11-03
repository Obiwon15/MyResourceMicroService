using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resourceedge.Common;
using Resourceedge.Common.Consul;
using Resourceedge.Common.Dispatchers;
using Resourceedge.Common.Handlers.Interfaces;
using Resourceedge.Common.Jaeger;
using Resourceedge.Common.Mongo;
using Resourceedge.Common.Mongo.Extensions;
using Resourceedge.Common.Mvc;
using Resourceedge.Common.RabbitMq;
using Resourceedge.Common.Redis;
using Resourceedge.Common.Swagger;
using Resourceedge.Operations.Api.Handlers;
using Resourceedge.Operations.Api.Services;
using Resourceedge.Operations.Api.Services.Interfaces;

namespace Resourceedge.Operations.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCustomMvc();
            services.AddSwaggerDocs(typeof(Startup).Assembly);
            services.AddConsul();
            services.AddJaeger();
            services.AddOpenTracing();
            services.AddRedis();
            //services.AddChronicle();
            services.AddIntializers(typeof(MongoDbInitializer));
            services.AddTransient<IOperationsStorage, OperationsStorage>();
            services.AddTransient<IOperationPublisher, OperationPublisher>();
            services.AddTransient(typeof(IEventHandler<>),typeof(GenericEventHandler<>));

            services.AddDefaultEdgeServices();
            services.AddDispatchers();
            services.AddRabbitMq(new Assembly[] { typeof(Startup).Assembly});
            services.AddMongo();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime, IConsulClient client,
            IStartupInitializer startupInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAllForwardedHeaders();
            app.UseSwaggerDocs();
            app.UseErrorHandler();
            app.UseServiceId();

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseRabbitMq()
                .SubscribeAllMessages();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var consulServiceId = app.UseConsul();
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                client.Agent.ServiceDeregister(consulServiceId);
            });

            startupInitializer.InitializeAsync();

        }
    }
}

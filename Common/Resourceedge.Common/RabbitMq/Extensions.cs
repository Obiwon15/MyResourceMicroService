using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTracing;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Configuration;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Instantiation;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using Resourceedge.Common.Handlers;
using Resourceedge.Common.Handlers.Interfaces;
using Resourceedge.Common.interfaces.RabbitMq;
using Resourceedge.Common.Jaeger;
using Resourceedge.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Resourceedge.Common.RabbitMq
{
    public static class Extensions
    {
        private static string _sectionName = "rabbitMq";
        public static IBusSubscriber UseRabbitMq(this IApplicationBuilder app)
           => new BusSubscriber(app);

        public static void AddRabbitMq(this IServiceCollection services, Assembly[] Startup)
        {
            IConfiguration configuration;

            using (ServiceProvider scope = services.BuildServiceProvider())
            {
                configuration = scope.GetService<IConfiguration>();
            }
            services.AddSingleton(sp => configuration.GetOptions<RabbitMqOptions>(_sectionName));
            services.AddSingleton(sp => configuration.GetOptions<RawRabbitConfiguration>(_sectionName));
            RegisterDynamicEvent(typeof(IEventHandler<>), Startup, services);
            RegisterDynamicCommand(typeof(ICommandHandler<>), Startup, services);

            //services.AddSingleton(typeof(IEventHandler<>));
            //services.AddSingleton(typeof(ICommandHandler<>));

            services.AddSingleton<IHandler, Handler>();
            services.AddSingleton<IBusPublisher, BusPublisher>();
            services.AddSingleton<ITracer>(cfg => ResourceedgeDefaultTracer.Create());
            ConfigureBus(configuration, services);
            //System.Diagnostics.Debug.WriteLine("rabbit mq check {0}", options.Hostnames[0]);
            //System.Diagnostics.Debug.WriteLine("Rabbit mq ccc {0}", options.Port);
        }

        private static void RegisterDynamicEvent(Type args, Assembly[] assemblies, IServiceCollection services)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(args)));
            foreach (var type in typesFromAssemblies)
                services.Add(new ServiceDescriptor(typeof(IEventHandler<>), type, ServiceLifetime.Singleton));
        }

        private static void RegisterDynamicCommand(Type args, Assembly[] assemblies, IServiceCollection services)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(args)));
            foreach (var type in typesFromAssemblies)
                services.Add(new ServiceDescriptor(typeof(ICommandHandler<>), type, ServiceLifetime.Singleton));
        }

        /// <summary>
        /// this was suppose to be used for registering all interfaces implementation dynamicall
        /// i couldn't find a way to register interface implementations with generic parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <param name="lifetime"></param>
        public static void RegisterAllTypes<T>(this IServiceCollection services, Assembly[] assemblies,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
            foreach (var type in typesFromAssemblies)
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
        }

        private static void ConfigureBus(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<IInstanceFactory>(context =>
            {
                var options = context.GetService<RabbitMqOptions>();
                var configuration = context.GetService<RawRabbitConfiguration>();
                var namingConventions = new CustomNamingConventions(options.Namespace);
                var tracer = context.GetService<ITracer>();
                return RawRabbitFactory.CreateInstanceFactory(new RawRabbitOptions
                {
                    DependencyInjection = ioc =>
                    {
                        ioc.AddSingleton(options);
                        ioc.AddSingleton(configuration);
                        ioc.AddSingleton<INamingConventions>(namingConventions);
                        ioc.AddSingleton(tracer);
                    },
                    Plugins = p => p
                    .UseAttributeRouting()
                    .UseRetryLater()
                    .UpdateRetryInfo()
                    .UseMessageContext<CorrelationContext>()
                    .UseContextForwarding()
                    .UseJaeger(tracer)
                });
            });

            services.AddSingleton(cfg =>
            {
                IInstanceFactory instance = cfg.GetService<IInstanceFactory>();
                    return instance.Create();
            });

        }

        private class CustomNamingConventions : NamingConventions
        {
            public CustomNamingConventions(string defaultNamespace)
            {
                var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                ExchangeNamingConvention = type => GetNamespace(type, defaultNamespace).ToLowerInvariant();
                RoutingKeyConvention = type =>
                    $"{GetRoutingKeyNamespace(type, defaultNamespace)}{type.Name.Underscore()}".ToLowerInvariant();
                QueueNamingConvention = type => GetQueueName(assemblyName, type, defaultNamespace);
                ErrorExchangeNamingConvention = () => $"{defaultNamespace}.error";
                RetryLaterExchangeConvention = span => $"{defaultNamespace}.retry";
                RetryLaterQueueNameConvetion = (exchange, span) =>
                    $"{defaultNamespace}.retry_for_{exchange.Replace(".", "_")}_in_{span.TotalMilliseconds}_ms".ToLowerInvariant();
            }
        }

        private static string GetNamespace(Type type, string defaultNamespace)
        {
            var @namespace = type.GetCustomAttribute<MessageNamespaceAttribute>()?.Namespace ?? defaultNamespace;

            return string.IsNullOrWhiteSpace(@namespace) ? type.Name.Underscore() : $"{@namespace}";
        }

        private static string GetRoutingKeyNamespace(Type type, string defaultNamespace)
        {
            var @namespace = type.GetCustomAttribute<MessageNamespaceAttribute>()?.Namespace ?? defaultNamespace;

            return string.IsNullOrWhiteSpace(@namespace) ? string.Empty : $"{@namespace}.";
        }

        private static string GetQueueName(string assemblyName, Type type, string defaultNamespace)
        {
            var @namespace = type.GetCustomAttribute<MessageNamespaceAttribute>()?.Namespace ?? defaultNamespace;
            var separatedNamespace = string.IsNullOrWhiteSpace(@namespace) ? string.Empty : $"{@namespace}.";

            return $"{assemblyName}/{separatedNamespace}{type.Name.Underscore()}".ToLowerInvariant();
        }
        private class RetryStagedMiddleware : StagedMiddleware
        {
            public override string StageMarker { get; } = RawRabbit.Pipe.StageMarker.MessageDeserialized;

            public override async Task InvokeAsync(IPipeContext context,
                CancellationToken token = new CancellationToken())
            {
                var retry = context.GetRetryInformation();
                if (context.GetMessageContext() is CorrelationContext message)
                {
                    message.Retries = retry.NumberOfRetries;
                }

                await Next.InvokeAsync(context, token);
            }
        }

        private static IClientBuilder UpdateRetryInfo(this IClientBuilder clientBuilder)
        {
            clientBuilder.Register(c => c.Use<RetryStagedMiddleware>());

            return clientBuilder;
        }
    }
}

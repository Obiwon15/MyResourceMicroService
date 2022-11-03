using Resourceedge.Common.interfaces.RabbitMq;
using Resourceedge.Common.Messages;
using Resourceedge.Operations.Api.Messages.Operations.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Resourceedge.Operations.Api
{
    public static class Subscriptions
    {
        private static readonly Assembly MessageAssembly = typeof(Subscriptions).Assembly;

        private static readonly ISet<Type> ExcludedMessages = new HashSet<Type>(new[]
        {
            typeof(OperationPending),
            typeof(OperationCompleted),
            typeof(OperationRejected)
        });

        public static IBusSubscriber SubscribeAllMessages(this IBusSubscriber subscriber)
               => subscriber.SubscribeAllCommands().SubscribeAllEvents();

        private static IBusSubscriber SubscribeAllCommands(this IBusSubscriber subscriber)
            => subscriber.SubscribeAllMessages<ICommand>(nameof(IBusSubscriber.SubscribeCommand));

        private static IBusSubscriber SubscribeAllEvents(this IBusSubscriber subscriber)
            => subscriber.SubscribeAllMessages<IEvent>(nameof(IBusSubscriber.SubscribeEvent));

        private static IBusSubscriber SubscribeAllMessages<TMessage>(this IBusSubscriber subscriber, string subcribeMethod) where TMessage : IMessage
        {
            var messageTypes = MessageAssembly
                .GetTypes().Where(t => t.IsClass && typeof(TMessage).IsAssignableFrom(t))
                .Where(t => !ExcludedMessages.Contains(t))
                .ToList();

            messageTypes.ForEach(mt => subscriber.GetType()
            .GetMethod(subcribeMethod)
            .MakeGenericMethod(mt)
            .Invoke(subscriber, new object[] { mt.GetCustomAttribute<MessageNamespaceAttribute>()?.Namespace, null, null }));

            return subscriber;
        }
    }
}

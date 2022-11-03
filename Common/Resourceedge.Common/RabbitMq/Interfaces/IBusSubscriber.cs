using Resourceedge.Common.Messages;
using Resourceedge.Common.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.interfaces.RabbitMq
{
    public interface IBusSubscriber
    {
        IBusSubscriber SubscribeCommand<TCommand>(string @namespace = null, string queueName = null,
            Func<TCommand, ResourceedgeException, IRejectedEvent> onError = null) where TCommand : ICommand;

        IBusSubscriber SubscribeEvent<TEvent>(string @namespace = null, string queueName = null,
            Func<TEvent, ResourceedgeException, IRejectedEvent> onError = null) where TEvent : IEvent;
    }
}

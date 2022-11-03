using Resourceedge.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.interfaces.RabbitMq
{
    /// <summary>
    /// This is used to publish either a Command or an Event
    ///  <c>
    /// IEvent and TCommand must both implement the ICommand Marker
    ///  </c>
    /// 
    /// 
    /// </summary>
    public interface IBusPublisher
    {
        Task SendAsync<TCommand>(TCommand command, ICorrelationContext context) where TCommand : ICommand;
        Task PublishAsync<TEvent>(TEvent @event, ICorrelationContext context) where TEvent : IEvent;
    }
}

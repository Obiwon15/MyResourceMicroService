using Resourceedge.Common.interfaces.RabbitMq;
using Resourceedge.Common.Messages;
using System.Threading.Tasks;

namespace Resourceedge.Common.Handlers.Interfaces
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event, ICorrelationContext context);
    }
}

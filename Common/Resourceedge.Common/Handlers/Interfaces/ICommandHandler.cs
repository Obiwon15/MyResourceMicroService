using Resourceedge.Common.interfaces.RabbitMq;
using Resourceedge.Common.Messages;
using System.Threading.Tasks;

namespace Resourceedge.Common.Handlers.Interfaces
{
   public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, ICorrelationContext context);
    }
}

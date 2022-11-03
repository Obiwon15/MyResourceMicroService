using Resourceedge.Common.Types.Interfaces;
using System.Threading.Tasks;

namespace Resourceedge.Common.Handlers.Interfaces
{
    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}

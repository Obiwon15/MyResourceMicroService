using Resourceedge.Common.Messages;
using Resourceedge.Common.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.Dispatchers.Interfaces
{
   public  interface IDispatcher
    {
        Task SendAsync<TCommand>(TCommand command) where TCommand : ICommand;
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query);
    }
}

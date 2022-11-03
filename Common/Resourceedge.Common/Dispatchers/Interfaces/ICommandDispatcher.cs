using Resourceedge.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common.Dispatchers.Interfaces
{
   public  interface ICommandDispatcher
    {
        Task SendAsync<T>(T command) where T : ICommand;
    }
}

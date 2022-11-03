using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common
{
    public interface IInitializer
    {
        Task InitializeAsync();
    }
}

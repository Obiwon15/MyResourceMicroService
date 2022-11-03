using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common
{
    public interface IStartupInitializer : IInitializer
    {
        void AddInitializer(IInitializer initializer);
    }
}

using Resourceedge.Common.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Types.Entities.Interfaces
{
    public interface IEntityBase<TId>  : IIdentifiable
    {
        TId Id { get; }
    }
}

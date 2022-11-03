using Resourceedge.Operations.Api.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Operations.Api.Services.Interfaces
{
    public interface IOperationsStorage
    {
        Task<OperationDto> GetAsync(Guid id);

        Task SetAsync(Guid id, string userId, string name, OperationState state,
            string resource, string code = null, string reason = null);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Resourceedge.Common.Dispatchers.Interfaces;
using Resourceedge.Operations.Api.Dto;
using Resourceedge.Operations.Api.Services.Interfaces;

namespace Resourceedge.Operations.Api.Controllers
{
    public class OperationsController : BaseController
    {
        private readonly IOperationsStorage _operationsStorage;

        public OperationsController(IDispatcher dispatcher,
            IOperationsStorage operationsStorage) : base(dispatcher)
        {
            _operationsStorage = operationsStorage;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OperationDto>> Get(Guid id)
            => Single(await _operationsStorage.GetAsync(id));
    }
}

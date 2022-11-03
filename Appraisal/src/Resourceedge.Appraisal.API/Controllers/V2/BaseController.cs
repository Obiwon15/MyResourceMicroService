using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Resourceedge.Appraisal.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Controllers.V2
{
    [Produces("application/json")]
    public class BaseController : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public override OkObjectResult Ok(object value)
        {
            var response = new ResponseResult
            {
                Data = value,
                Success = true
            };

            return base.Ok(response);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public new NotFoundObjectResult NotFound()
        {
            return NotFound("Resource not found");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public NotFoundObjectResult NotFound(string message)
        {
            var response = new ResponseResult
            {
                Success = false,
                Errors = new List<string> { message }
            };

            return base.NotFound(response);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public OkObjectResult Error(IEnumerable<string> errors)
        {
            var response = new ResponseResult
            {
                Errors = errors,
                Success = false
            };

            return base.Ok(response);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public OkObjectResult Invalid()
        {
            var response = new ResponseResult
            {
                Errors = ModelState.Values
                .SelectMany(x => x.Errors.Select(err => err.ErrorMessage))
                .ToList(),
                Success = false
            };

            return base.Ok(response);
        }

    }
}

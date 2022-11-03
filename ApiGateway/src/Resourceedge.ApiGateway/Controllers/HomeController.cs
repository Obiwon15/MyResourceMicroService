using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Resourceedge.ApiGateway.Controllers
{
    [Route("", Name ="Home")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index() => Ok("Resourceedge API");
    }
}

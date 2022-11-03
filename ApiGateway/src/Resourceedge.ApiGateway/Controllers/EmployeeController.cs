using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTracing;
using Resourceedge.ApiGateway.Services.Employee;
using Resourceedge.Common.interfaces.RabbitMq;

namespace Resourceedge.ApiGateway.Controllers
{
    [Route("[controller]")]
    public class EmployeeController : BaseController
    {
        public IBusPublisher _busPublisher;
        public ITracer _tracer;
        public IEmployeeService _employeeHttpService;
        public EmployeeController(IBusPublisher busPublisher, ITracer tracer, IEmployeeService employeeHttpService) : base(busPublisher, tracer)
        {
            _busPublisher = busPublisher;
            _tracer = tracer;
            _employeeHttpService = employeeHttpService;
        }

        public IActionResult Index()
        {
            return Ok();
        }

        [Route("{Id}", Name ="Get Employee By Name")]
        [HttpGet]
        public async Task<object> GetEmployeeById(string Id)
        {
            var result = await _employeeHttpService.GetEmployeeByIdAsync(Id);
            return Ok(result);
        }
    }
}

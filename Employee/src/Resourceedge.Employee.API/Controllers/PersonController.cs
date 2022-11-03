using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Resourceedge.Common.Dispatchers.Interfaces;
using Resourceedge.Common.Mvc;
using Resourceedge.Employee.Application.Commands;

namespace Resourceedge.Employee.API.Controllers
{
    [ApiController]
    [Route("api/person")]
    public class PersonController : Controller
    {
        private readonly IDispatcher _dispatcher;

        public PersonController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePerson command)
        {
             command.Bind(x => x.Id, ObjectId.GenerateNewId().ToString());
            await _dispatcher.SendAsync(command);
            return Accepted();
        }

        [HttpGet("GenerateId")]
        private CreatePerson generateId(CreatePerson x)
        {
            //x.Id = ObjectId.GenerateNewId().ToString();
            return x;
        }
    }
}

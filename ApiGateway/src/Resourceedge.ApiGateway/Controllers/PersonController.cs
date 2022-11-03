using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;
using Resourceedge.ApiGateway.Messages.Commands.Persons;
using Resourceedge.Common.interfaces.RabbitMq;
using Resourceedge.Common.Mvc;

namespace Resourceedge.ApiGateway.Controllers
{

    public class PersonController : BaseController
    {
        private readonly IBusPublisher _busPublisher;
        private readonly ITracer _tracer;

        public PersonController(IBusPublisher busPublisher, ITracer tracer) : base(busPublisher, tracer)
        {
            _busPublisher = busPublisher;
            _tracer = tracer;
        }

        [HttpPost]
        public async Task AddPerson(CreatePerson command)
        {
            command.BindId(x => x.Id);
            await SendAsync(command, resourceId: command.Id, resource: "person");
        }
    }
}

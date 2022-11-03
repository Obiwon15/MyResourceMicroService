using MongoDB.Bson;
using Resourceedge.Common.Handlers.Interfaces;
using Resourceedge.Common.interfaces.RabbitMq;
using Resourceedge.Employee.Application.Commands;
using Resourceedge.Employee.Application.Events;
using Resourceedge.Employee.Domain.Entities;
using Resourceedge.Employee.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Employee.Application.Handlers
{
    public class CreatePersonHandler : ICommandHandler<CreatePerson>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IBusPublisher _busPublisher;

        public CreatePersonHandler(IPersonRepository personRepository, IBusPublisher busPublisher)
        {
            _personRepository = personRepository;
            _busPublisher = busPublisher;
        }

        public async Task HandleAsync(CreatePerson command, ICorrelationContext context)
        {
            var person = new Person(command.Id, command.FirstName, command.LastName, command.Id);
            await _personRepository.AddPerson(person);

            await _busPublisher.PublishAsync(new PersonCreated(command.FirstName, command.LastName), context);
        }
    }
}

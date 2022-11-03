using Resourceedge.Common.Mongo;
using Resourceedge.Common.Mongo.Interfaces;
using Resourceedge.Employee.Domain.Entities;
using Resourceedge.Employee.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Employee.Infrastructure.Repositories
{
    public class PersonRepository :  IPersonRepository
    {
        private readonly IMongoRepository<Person> _personRepository;
        public PersonRepository(IMongoRepository<Person> personRepository)
        {
            _personRepository = personRepository;
           
        }

        public async Task AddPerson(Person person)
        {
            await _personRepository.AddAsync(person);
        }
    }
}

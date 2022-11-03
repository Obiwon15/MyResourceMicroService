using Resourceedge.Common.Mongo.Interfaces;
using Resourceedge.Employee.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Employee.Domain.Interfaces
{

    public interface IPersonRepository
    {
        Task AddPerson(Person person); 
    }
}

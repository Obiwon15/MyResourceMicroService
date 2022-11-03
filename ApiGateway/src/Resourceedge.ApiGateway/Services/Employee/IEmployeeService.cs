using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.ApiGateway.Services.Employee
{
    [SerializationMethods(Query = QuerySerializationMethod.Serialized)]
    public interface IEmployeeService
    {
        [AllowAnyStatusCode]
        [Get("api/employee/new/{id}")]
        Task<object> GetEmployeeByIdAsync([Path] string id);
    }
}

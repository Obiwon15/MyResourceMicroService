using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Employee.API.Services.HttpServices
{
    public interface ITestHttpService
    {
        [AllowAnyStatusCode]
        [Get("products/{id}")]
        Task<string> GetAsync([Path] Guid id);
    }
}

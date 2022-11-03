using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Resourceedge.Common.Archive;
using Resourceedge.Common.Util;
using Resourceedge.Employee.Domain.Entities;
using Resourceedge.Employee.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Resourceedge.Employee.Domain.Interfaces
{
    public interface IEmployee
    {
        bool InsertEmployeeBioData(EmployeeBioData employeeBioData);
        Task<EmployeeBioDataDto> GetEmployeeBioData(int? empId);
        bool HasFilledBiodata(int empId);
        Task<EmployeeBioData> UpdateEmployeeBioData(int empId, JsonPatchDocument<EmployeeBioDataDto> model);
        Tuple<PagedList<OldEmployeeForViewDto>, int> Get(int pageNumber, int pageSize, string company, string searchParam);
        IEnumerable<string> GetOrganisation();
        Task<(bool, string)> AddNewEmployee(EmployeeCreateDto employee);
        Task<bool> AddNewEmployeeMany(IEnumerable<EmployeeCreateDto> employees);
        bool CheckIfExcelFile(IFormFile file);
        Task<IEnumerable<EmployeeCreateDto>> ReadExcel(IFormFile file);
    }
}

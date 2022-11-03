using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Resourceedge.Employee.Domain.Interfaces;
using Resourceedge.Employee.Domain.Models;
using System.Threading.Tasks;

namespace Resourceedge.Employee.API.Controllers
{
    [Route("api/biodata")]
    [ApiController]
    public class ManageEmployeeController : ControllerBase
    {
        private readonly IEmployee bioDataRepo;
        private readonly IMapper mapper;

        public ManageEmployeeController(IEmployee _bioDataRepo, IMapper _mapper)
        {
            bioDataRepo = _bioDataRepo;
            mapper = _mapper;
        }

        [HttpGet("index", Name = "Index")]
        public async Task<IActionResult> EmployeeList(int pageSize, int pageNumber, string company, string searchParam)
        {
            var data = bioDataRepo.Get(pageNumber, pageSize, company, searchParam);
            return Ok(data);
        }

        [HttpGet("Company")]
        public IActionResult GetCompany()
        {
            return Ok(new { data = bioDataRepo.GetOrganisation() });
        }

        [HttpPost("AddEmployee")]
        public async Task<IActionResult> AddEmployee(EmployeeCreateDto employee)
        {
            if (employee is null)
                return BadRequest(new { status = false, message = "Invalid data passed, Try again" });

            var res = await bioDataRepo.AddNewEmployee(employee);
            if (!res.Item1)
                return BadRequest(new { status = false, message = res.Item2 });

            return CreatedAtRoute("Index", employee);
        }

        [HttpPost("ImportEmployees")]
        public async Task<IActionResult> AddEmployeeBulk(IFormFile file)
        {
            if (string.IsNullOrEmpty(file.FileName))
                return BadRequest(new { message = "Ensure you are passing a file" });

            if (!bioDataRepo.CheckIfExcelFile(file))
                return BadRequest(new { message = "File not an excel, Please upload an excel" });

            var employeesToUpload = await bioDataRepo.ReadExcel(file);
            if (employeesToUpload == null)
                return BadRequest(new { message = "No employee in uploaded excel, Make sure your headers are in place Or something went error" });

            var result = await bioDataRepo.AddNewEmployeeMany(employeesToUpload);
            if (!result)
                return BadRequest(new { message = "Error, Something went wrong" });

            return Ok(new { status = "true", message = "Employees Uploaded successfull" });
        }
    }
}

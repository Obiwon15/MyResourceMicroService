using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Resourceedge.Employee.Domain.Entities;
using Resourceedge.Employee.Domain.Interfaces;
using Resourceedge.Employee.Domain.Models;

namespace Resourceedge.Employee.API.Controllers
{
    [Route("api/biodata/{empId:int}")]
    [ApiController]
    public class EmployeeBioDataController : ControllerBase
    {
        private readonly IEmployee bioDataRepo;
        private readonly IMapper mapper;
        private readonly IOldEmployee employeeRepo;

        public EmployeeBioDataController(IEmployee _bioDataRepo, IMapper _mapper, IOldEmployee _employeeRepo)
        {
            bioDataRepo = _bioDataRepo;
            mapper = _mapper;
            employeeRepo = _employeeRepo;
        }

        [HttpGet(Name = "MyBio")]
        public async Task<IActionResult> GetEmployeeBioData(int empId)
        {
            if (employeeRepo.GetEmployeeByEmployeeId(empId) == null)
                return NotFound(new { status = false, message = " Employee not found" });

            var result = await bioDataRepo.GetEmployeeBioData(empId);
            return Ok(result);
        }

        [HttpPatch("UpdateBioData")]
        public async Task<IActionResult> AddEmployeeBioData(int empId, JsonPatchDocument<EmployeeBioDataDto> model)
        {
            if (employeeRepo.GetEmployeeByEmployeeId(empId) == null)
                return NotFound(new { status = false, message = " Employee not found" });

            try
            {
                var result = await bioDataRepo.UpdateEmployeeBioData(empId, model);

                var entityToReturn = mapper.Map<EmployeeBioDataDto>(result);

                return CreatedAtRoute("MyBio", new { empId = empId }, model);
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpPost("AddBioData")]
        public IActionResult AddEmployeeBioData(int empId, EmployeeBioDataDto model)
        {
            if (bioDataRepo.HasFilledBiodata(empId))
                return NotFound(new { status = false, message = " Employee Already Exist" });

            try
            {
                var entityToAdd = mapper.Map<EmployeeBioData>(model);

                var result = bioDataRepo.InsertEmployeeBioData(entityToAdd);

                var entityToReturn = mapper.Map<EmployeeBioDataDto>(result);

                return CreatedAtRoute("MyBio", new { empId = empId }, model);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet("HasFilledBioData")]
        public async Task<IActionResult> HasFilledBioData(int empId)
        {
            if (employeeRepo.GetEmployeeByEmployeeId(empId) != null)
                return NotFound(new { status = false, message = " Employee not found" });

            if (bioDataRepo.HasFilledBiodata(empId))
                return Ok(new { status = true, message = "Employee has Filled Bio data" });
            else
                return Ok(new { status = false, message = "Employee has not filled bio data" });
        }     
    }
}

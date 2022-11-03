using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Resourceedge.Appraisal.API.Controllers.V2;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Appraisal.Domain.Queries;
using Resourceedge.Common.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Controllers
{
    [Authorize]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [ApiController]
    [Route("api/v{version:apiVersion}/Appraisal/")]
    public class AppraisalResultController : BaseController
    {
        private readonly IAppraisalResult appraisalResult;
        private readonly IMapper mapper;
        

        public AppraisalResultController(IAppraisalResult _appraisalResult, IMapper _mapper)
        {
            appraisalResult = _appraisalResult;
            mapper = _mapper;
          
        }

        [HttpGet("")]
        public IActionResult EmployeeAppraisal(int? employee, string appraisalConfig, string appraisalCycle)
        {

            if (employee == null || appraisalConfig == null || appraisalCycle == null)
            {

                return BadRequest();
            }

            ObjectId configId = new ObjectId(appraisalConfig);
            ObjectId cycleId = new ObjectId(appraisalCycle);

            var result = appraisalResult.Get(configId, cycleId, employee);

            return Ok(result);
        }

        [HttpPost("ApprovalAppraisal/{hodId:int}")]
        public async Task<IActionResult> ApproveAppraisalHOD(int hodId, [FromQuery] AppraisalPerformanceParam param, [FromBody] HodApprovalDto approvalStatus)
        {
            var result = await appraisalResult.ApproveAppraisalHod(hodId, param, approvalStatus);

            return Ok(result);
        }

        [HttpGet("hasparticipated")]
        public async Task<IActionResult> HasParticipated(int employeeId, string reviewId)
        {
            ObjectId appraisalReviewId = ObjectId.Parse(reviewId);
            var result = await appraisalResult.HasPaticipatedInAppraisal(employeeId, appraisalReviewId);
            return Ok(result);
        }

        [HttpGet, Route("directReport")]
        [ProducesResponseType(typeof(DirectReportDto), 200)]
        public async Task<IActionResult> GetEmployeesWithSubmittedAppraisal([FromQuery] AppraisalPerformanceParam performanceParam, string whoami, [FromQuery]PaginationResourceParameter pagination)
        {
            var employeesToAppraise = await appraisalResult.SearchEmployeeToAppraise(performanceParam.empId, performanceParam.reviewId, whoami, pagination);

            return Ok(employeesToAppraise);
        }

        [HttpGet, Route("kraforappraisal")]
        public async Task<IActionResult> GetKraforAppraisal([FromQuery] AppraisalPerformanceParam param)
        {
            var resultFromMap = await appraisalResult.GetAcceptedKRAForAppraisal(param);
            var mapInstance = mapper.Map<IEnumerable<AppraisalKeyResultAreaForViewDto>>(resultFromMap).ToList();

            mapInstance.RemoveAll(x => !x.keyOutcomes.Any());
            return Ok(mapInstance);
        }

        [HttpGet("Updatekeyresultarea")]
        public async Task<IActionResult> UpdateExistingAppraisalResult([FromQuery] AppraisalQueryParam configParam)
        {
            var configDetails = await appraisalResult.GetAppraisalConfiguration(configParam.Config);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }
            var result = await appraisalResult.UpdateKeyResultAreaForExistingResult(configParam.Cycle);
            if (result)
            {
                return Ok(new { success = "Update completed" });
            }

            return BadRequest(new { message = "something went wrong" });
        }

        [HttpGet("ResetAppraisal/{AppraiserId}/{EmployeeId}")]
        public async Task<IActionResult> ResetAppraisal(int AppraiserId, int EmployeeId, [FromQuery] AppraisalQueryParam configParam)
        {
            var configDetails = await appraisalResult.GetAppraisalConfiguration(configParam.Config);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }
            var result = await appraisalResult.RestAppraisal(EmployeeId, AppraiserId, ObjectId.Parse(configParam.Cycle));
            if (result)
            {
                return Ok(new { success = "Update completed" });
            }

            return BadRequest(new { message = "something went wrong" });
        }

        [HttpDelete("ResetEmployeeAppraisal/{EmployeeId}")]
        public async Task<IActionResult> ResetEmployeeAppraisal(int EmployeeId, [FromQuery] AppraisalQueryParam configParam)
        {
            var configDetails = await appraisalResult.GetAppraisalConfiguration(configParam.Config);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }

            var result = await appraisalResult.ResetEmployeeAppraisal(EmployeeId, ObjectId.Parse(configParam.Cycle));
            if (result)
            {
                return Ok(result);
            }

            return BadRequest();
        }

        [HttpGet("CheckForRejectedAppraisal")]
        public async Task<IActionResult> CheckForRejectedAppriasal([FromQuery]AppraisalPerformanceParam param)
        {
            var result = await appraisalResult.GetRejectedEmployeeAppraisal(param.empId, ObjectId.Parse(param.reviewId));

            return Ok(result);
        }
    }
}

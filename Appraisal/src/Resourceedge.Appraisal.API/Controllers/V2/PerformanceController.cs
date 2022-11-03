using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Common.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Controllers.V2
{
    //[Authorize]
    [ApiController, ApiVersion("2")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PerformanceController : BaseController
    {
        private readonly IServiceFactory _serviceFactory;
        private readonly IAppraisalResult _appraisalResult;

        public PerformanceController(IAppraisalResult appraisalResult, IServiceFactory serviceFactory)
        {
            _appraisalResult = appraisalResult;
            _serviceFactory = serviceFactory;
        }

        [HttpGet("AppraisalParticipants")]
        public async Task<IActionResult> GetAppraisalParticipants([FromQuery] string cycleId, [FromQuery] PaginationResourceParameter param)
        {
            var result = await _appraisalResult.GetEmployeesParticipatingInAppraisal(ObjectId.Parse(cycleId), param);

            return Ok(result);
        }

        [HttpGet("")]
        //[ProducesResponseType(typeof(IEnumerable<AppraisalResult>), 200)]
        public async Task<IActionResult> EmployeeAppraisal([FromQuery] AppraisalPerformanceParam param)
        {
            var result = await _appraisalResult.Get(param.reviewId, param.empId);

            return Ok(result);
        }

        [HttpPost("selfAppraisal")]
        public async Task<IActionResult> AppraiseSelf([FromQuery] AppraisalPerformanceParam param, [FromBody] IEnumerable<AppraisalPerformanceDto> appraisalResultForCreation)
        {

            var appraisalResultToSubmit = await _appraisalResult.ValidateSubmission(param.empId, param.reviewId, appraisalResultForCreation);

            var result = await _appraisalResult.SubmitAppraisal(param.empId, ObjectId.Parse(param.reviewId), appraisalResultToSubmit);

            return Ok(result);
        }

        [HttpPut("AppraiseEmployee")]
        public async Task<IActionResult> AppraiseEmployee([FromQuery] AppraisalPerformanceParam param, [FromBody] IEnumerable<AppraisalPerformanceDto> appraisalResultForCreation)
        {
            var appraisalResultToSubmit = await _appraisalResult.ValidateSubmission(param.empId, param.reviewId, appraisalResultForCreation);

            var result = await _appraisalResult.AppraiseEmployee(param.empId, ObjectId.Parse(param.reviewId), appraisalResultToSubmit);

            return Ok(result);
        }

        [HttpPut("AcceptAppraisal")]
        public async Task<IActionResult> AcceptAppraisal([FromQuery] AppraisalPerformanceParam param, AcceptanceStatus entity)
        {
            await _appraisalResult.EmployeeAcceptOrReject(param.empId, ObjectId.Parse(param.reviewId), entity);
            return Ok();
        }

        [HttpGet("EmployeeReviews")]
        public IActionResult GetEmployeeReviews(int employeeId)
        {
            var employeeReviews = _appraisalResult.GetEmployeeReviews(employeeId);

            return Ok(employeeReviews);
        }
    }
}

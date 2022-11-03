using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.API.ResourceParamters;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Controllers.V2
{
    [Authorize]
    [ApiController, ApiVersion("2")]
    [Route("api/v{version:apiVersion}/Appraisal/c")]
    public class AppraisalConfigurationController : BaseController
    {
        private readonly IServiceFactory _serviceFactory;
        private readonly IAppraisalConfig _appraisalConfigServices;

        public AppraisalConfigurationController(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _appraisalConfigServices = _serviceFactory.GetServices<IAppraisalConfig>();
        }

        [HttpGet("Reviews")]
        [ProducesResponseType(typeof(IEnumerable<ConfigAppraisalViewDto>), 200)]
        public async Task<IActionResult> Reviews([FromQuery] AppraisalConfigParameters param)
        {
            var result = await _appraisalConfigServices.GetReviews(param);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Review([FromBody] ConfigAppraisalDto configDto)
        {
            var result = _appraisalConfigServices.InsertReview(configDto);

            return Ok(result);
        }

        [HttpGet("aa")]
        public async Task<IActionResult> Get() => Ok("Appraisal Get");

        [HttpPut("Enable/{id}")]
        public async Task<IActionResult> EnableOrDisableAppraisal(string id)
        {
            var result = await _appraisalConfigServices.EnableOrDisableAppraisal(new ObjectId(id));

            return Ok(result);
        }

        [HttpPut("ArchiveAppraisal/{id}")]
        public async Task<IActionResult> ArchiveAppraisal(string id)
        {
            var result = await _appraisalConfigServices.ArchiveAppraisal(new ObjectId(id));

            return Ok(result);
        }

        [HttpPatch("UpdateReview/{Id}")]
        public IActionResult EditReview(string Id, JsonPatchDocument<AppraisalConfigForUpdateDto> model)
        {
            _appraisalConfigServices.UpdateAppraisalReview(Id, model);
           
            return Ok("Review Updated Successfully");
        }

        [HttpGet("Review/{Id}")]
        public async Task<IActionResult> SingleReview(string Id)
        {
            var result = await _appraisalConfigServices.GetSingleReview(Id);
            return Ok(result);
        }

        [HttpGet("ReviewType/{Id}")]
        public async Task<IActionResult> AppraisalType(string Id)
        {
            var result = await _appraisalConfigServices.GetReviewType(ObjectId.Parse(Id));
            return Ok(result);
        }

        [HttpPost("ReviewType")]
        public async Task<IActionResult> AddAppraisalType(string name)
        {
            var result = _appraisalConfigServices.InsertReviewType(name);
            return Ok(result);
        }

        [HttpGet("ReviewTypes")]
        public async Task<IActionResult> AllAppraisalTypes()
        {
            var result = await _appraisalConfigServices.GetReviewTypes();
            return Ok(result);
        }

        [HttpGet("EmployeeReviews")]
        public async Task<IActionResult> EmployeeReview(int employeeId, int? year)
        {
            var result = await _appraisalConfigServices.GetEmployeeReviews(employeeId, year);

            return Ok(result);
        }

    }
}

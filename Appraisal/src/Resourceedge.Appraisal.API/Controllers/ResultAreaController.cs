using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Resourceedge.Appraisal.API.Controllers.V2;
using Resourceedge.Appraisal.Domain.Queries;
using Resourceedge.Common.Util;
using Microsoft.AspNetCore.Authorization;

namespace Resourceedge.Appraisal.API.Controllers
{
    [Authorize]
    [ApiController, ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]/{empId:int}")]

    public class ResultAreaController : BaseController
    {
        private readonly IMapper mapper;
        private readonly IKeyResultArea resultArea;

        public ResultAreaController(IKeyResultArea _resultArea, IMapper _mapper)
        {
            this.resultArea = _resultArea;
            mapper = _mapper;
        }

        [HttpGet("Index")]
        [Obsolete("Deprecated")]
        public async Task<IActionResult> Index(int? pageSize, int pageNumber)
        {
            var data = await resultArea.Get(pageSize, pageNumber);
            return Ok(data);
        }

        [ApiVersion("2")]
        [HttpGet(Name = "Mykpi")]
        public async Task<IActionResult> GetPersonalKpis(int empId)
        {
            var resultFromMap = resultArea.GetPersonalkpis(empId);
            var mapInstance = mapper.Map<IEnumerable<AppraisalKeyResultAreaForViewDto>>(resultFromMap);
            return Ok(mapInstance);
        }

        [ApiVersion("2")]
        [HttpGet("{KeyResultAreaId}", Name = "GetEmployeeKpiById")]
        public ActionResult GetEmployeeKpiById(int empId, string KeyResultArea)
        {
            var data = resultArea.GetPersonalkpis(empId, KeyResultArea).FirstOrDefault();
            return Ok(data);
        }

        [ApiVersion("2")]
        [HttpPost("", Name = "CreateKeyOutcomes")]
        public async Task<IActionResult> CreateKeyResultArea(string empId, IEnumerable<KeyResultAreaDtoForCreation> model)
        {
            try
            {
                await resultArea.CreateKeyResultArea(empId, model);

                return Ok(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [ApiVersion("2")]
        [HttpPatch("Update/{keyResultAreaId}")]
        public async Task<IActionResult> UpdateKPI(int empId, string keyResultAreaId, JsonPatchDocument<KeyResultAreaForUpdateDto> entityForUpdate)
        {
            var result = await resultArea.UpdateKeyResultArea(empId, keyResultAreaId, entityForUpdate);
            if (!result.Item2)
            {
                return NotFound();
            }

            return Ok(result.Item1);
        }

        [HttpPost("Update/{KeyResultAreaId}")]
        public async Task<IActionResult> UpdateKPIs(int empId, string KeyResultAreaId, KeyResultAreaForUpdateDto entityForUpdate)
        {
            ObjectId Id = new ObjectId(KeyResultAreaId);
            var keyResult = await resultArea.QuerySingleByUserId(Id, empId);

            if (keyResult != null)
            {
                var resultAreaForUpdate = mapper.Map<KeyResultAreaForUpdateMainDto>(entityForUpdate);

                var entityToUpdate = resultArea.Update(Id, resultAreaForUpdate);
                var entityToReturn = mapper.Map<KeyResultAreaDtoForCreation>(entityToUpdate);

                return CreatedAtRoute("Mykpi", new { empId }, entityToReturn);
            }

            return NotFound();
        }

        [ApiVersion("2")]
        [HttpPatch("{KeyResultAreaId}/KeyOutcome/{KeyOutcomeId}")]
        public async Task<IActionResult> UpdateKeyOutcome(int empId, string KeyResultAreaId, string KeyOutcomeId, JsonPatchDocument<KeyOutcomeForUpdateDto> entityForUpdate)
        {
            var keyOutcomeForUpdate = new KeyOutcomeForUpdateDto();
            entityForUpdate.ApplyTo(keyOutcomeForUpdate);

            ObjectId Id = new ObjectId(KeyResultAreaId);
            ObjectId keyOutcomeId = new ObjectId(KeyOutcomeId);

            var result = await resultArea.UpdateKeyOutcome(Id, keyOutcomeId, empId, keyOutcomeForUpdate);
            if (result > 0)
            {
                return Ok(result);
            }

            return NotFound();
        }

        [HttpPost("member/{memberId}/{KeyResultAreaId}/Approval/{whoami}")]
        public async Task<IActionResult> ApprovalKeyOutCome(int empId, int memberId, string KeyResultAreaId, string whoami, StatusForUpdateDto entity)
        {
            var keyResultAreaId = new ObjectId(KeyResultAreaId);

            if (entity == null)
            {
                return BadRequest();
            }

            var result = await resultArea.HodApproval(empId, memberId, keyResultAreaId, whoami, entity);
            if (result != null)
            {
                return Ok(result);
            }

            return NotFound();
        }

        [ApiVersion("2")]
        [HttpPost("Approval")]
        public async Task<IActionResult> ApprovalKeyOutCome(int empId, StatusForUpdateDto entity)
        {
            if (entity == null)
            {
                return BadRequest();
            }

            var result = await resultArea.EmployeeApproval(empId, entity);
            if (result > 0)
            {
                return Ok(true);
            }
            return NoContent();
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteKeyResultArea(string empId, string Id)
        {
            ObjectId objId = new ObjectId(Id);
            var keyResult = await resultArea.QuerySingle(objId);

            if (keyResult != null)
            {
                resultArea.Delete(objId);

                return Ok(new { success = true });
            }
            return NoContent();
        }

        [HttpDelete("Delete/{Id}")]
        public async Task<IActionResult> DeleteKeyOutcome(string empId, string Id)
        {
            ObjectId objId = new ObjectId(Id);
            var keyResult = await resultArea.QuerySingleByKeyOutcome(objId);

            if (keyResult != null)
            {
                var result = await resultArea.DeleteKeyOutcome(objId, keyResult);
                if (result == null)
                {
                    return BadRequest();
                }

                return Ok(new { success = true, result });
            }

            return NoContent();
        }

        [ApiVersion("2")]
        [HttpGet("checkuploaded")]
        public IActionResult CheckUserUploadedEpaForYear(int empId)
        {
            var result = resultArea.HasUploadedEpa(empId);
            return Ok(result);
        }

        //[HttpGet("kraforappraisal")]
        //public ActionResult<IEnumerable<KeyResultAreaDtoForCreation>> GetKraforAppraisal(int empId,[FromQuery]AppraisalQueryParam configParam)
        //{
        //    var resultFromMap = resultArea.GetAcceptedAppraisal(empId, configParam);
        //    var mapInstance = mapper.Map<IEnumerable<KeyResultAreaForViewDto>>(resultFromMap);
        //    return Ok(mapInstance);
        //}    
        
        [HttpPost("ChangeSupervisor")]
        public async Task<IActionResult> ChangeSupervisor(int empId, ChangeSupervisorDtoForView supervisor)
        { 
            try
            {
                if (supervisor == null)
                {
                    return BadRequest();
                }

                ChangeSupervisorDto supervisorToChange = mapper.Map<ChangeSupervisorDto>(supervisor);

                var whoToUpdate = resultArea.CheckForSupervisor(empId, supervisorToChange.OldSupervisorId, supervisorToChange.KeyResultAreaId);
                if (string.IsNullOrEmpty(whoToUpdate))
                {
                    return NotFound();
                }

                var result = await resultArea.ChangeSupervisor(empId, supervisorToChange.NewSupervisorId, supervisorToChange.KeyResultAreaId, whoToUpdate);

                if (result) 
                    return Ok(new { message = "Supervisor updated !!!", status = true });

            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message});
            }

            return NoContent();
        }

        [ApiVersion("2")]
        [HttpDelete("ResetEPA")]
        public async Task<IActionResult> DeleteEmployeeEPA(int empId)
        {            
            await resultArea.DeleteEpa(empId);

            return Ok();
        }

        [HttpPost("ChangeAllSupervisor")]
        public async Task<IActionResult> ChangeAllSupervisor(int empId, ChangeSupervisorDtoForView supervisor)
        {
            try
            {
                if (supervisor == null)
                {
                    return BadRequest();
                }

                ChangeSupervisorDto supervisorToChange = mapper.Map<ChangeSupervisorDto>(supervisor);
                if (string.IsNullOrEmpty(supervisorToChange.WhoToUpdate))
                {
                    supervisorToChange.WhoToUpdate = resultArea.CheckForSupervisor(empId, supervisorToChange.OldSupervisorId, supervisorToChange.KeyResultAreaId);
                    if (string.IsNullOrEmpty(supervisorToChange.WhoToUpdate))
                    {
                        return NotFound();
                    }
                }               

                var result = await resultArea.ChangeSupervisor(empId, supervisorToChange);

                if (result)
                    return Ok(new { message = "Supervisor updated !!!", status = true });

            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPost("Restore")]
        public async Task<IActionResult> RestoreEpa(int empId, AppraisalQueryParam appraisalQuery)
        {
           var res = await resultArea.RestoreEPA(empId, appraisalQuery);

            if (res)
            {
                return Ok(new { status = true, message = "Restored" });
            }

            return NotFound();
        }

    }
}
